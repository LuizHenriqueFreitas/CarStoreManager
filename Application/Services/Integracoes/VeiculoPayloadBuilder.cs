using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Application.Services.Integracoes;

/// <summary>
/// Monta o payload de publicação para VeiculoVenda e VeiculoConsignacao — os
/// dois têm exatamente os mesmos campos descritivos (marca, modelo, ano, cor,
/// câmbio, combustível, quilometragem), então um construtor só atende ambos.
/// Veículo é sempre classificado no ML: sem shipping, quantidade fixa em 1,
/// e uma lista de atributos obrigatórios que varia por categoria — resolvida
/// aqui a partir do que o cadastro do veículo realmente tem.
/// </summary>
public class VeiculoPayloadBuilder : IConstrutorPayloadAnuncio
{
    private readonly IVeiculoVendaRepository _veiculoVendaRepo;
    private readonly IVeiculoConsignacaoRepository _veiculoConsignacaoRepo;
    private readonly IFotoService _fotoService;
    private readonly MercadoLivreCatalogoService _catalogo;
    private readonly MercadoLivreConfig _config;

    public VeiculoPayloadBuilder(
        IVeiculoVendaRepository veiculoVendaRepo,
        IVeiculoConsignacaoRepository veiculoConsignacaoRepo,
        IFotoService fotoService,
        MercadoLivreCatalogoService catalogo,
        IOptions<MercadoLivreConfig> config)
    {
        _veiculoVendaRepo = veiculoVendaRepo;
        _veiculoConsignacaoRepo = veiculoConsignacaoRepo;
        _fotoService = fotoService;
        _catalogo = catalogo;
        _config = config.Value;
    }

    public bool Aceita(string entidadeTipo) => entidadeTipo is "VeiculoVenda" or "VeiculoConsignacao";

    public async Task<Result<PayloadAnuncioMLDTO>> ConstruirAsync(
        string entidadeTipo, Guid entidadeId, string? categoriaOverride, string accessToken, string? mercadoLivreUserId)
    {
        string marca, modelo, cor, cambioTexto, combustivelTexto;
        int ano, km;
        decimal preco;
        string descricao;

        if (entidadeTipo == "VeiculoVenda")
        {
            var veiculo = await _veiculoVendaRepo.GetByIdAsync(entidadeId);
            if (veiculo is null) return Result<PayloadAnuncioMLDTO>.Fail("Veículo não encontrado");

            marca = veiculo.GetMarca(); modelo = veiculo.GetModelo(); cor = veiculo.GetCor();
            ano = veiculo.GetAno(); km = veiculo.GetQuilometragem();
            cambioTexto = TraduzirCambio(veiculo.GetCambio());
            combustivelTexto = TraduzirCombustivel(veiculo.GetCombustivel());
            preco = veiculo.GetValor();
            descricao = string.IsNullOrWhiteSpace(veiculo.TextoTermoPreliminar)
                ? "Veículo em perfeitas condições." : veiculo.TextoTermoPreliminar;
        }
        else
        {
            var veiculo = await _veiculoConsignacaoRepo.GetByIdAsync(entidadeId);
            if (veiculo is null) return Result<PayloadAnuncioMLDTO>.Fail("Veículo consignado não encontrado");

            marca = veiculo.GetMarca(); modelo = veiculo.GetModelo(); cor = veiculo.GetCor();
            ano = veiculo.GetAno(); km = veiculo.GetQuilometragem();
            cambioTexto = TraduzirCambio(veiculo.GetCambio());
            combustivelTexto = TraduzirCombustivel(veiculo.GetCombustivel());
            preco = veiculo.Comissao.ValorVendaEsperado.GetValorDinheiro();
            descricao = string.IsNullOrWhiteSpace(veiculo.TextoContrato)
                ? "Veículo consignado em perfeitas condições." : veiculo.TextoContrato;
        }

        var titulo = $"{marca} {modelo} {ano}";

        var categoriaId = categoriaOverride;
        if (string.IsNullOrWhiteSpace(categoriaId))
        {
            var sugestaoResult = await _catalogo.SugerirCategoriaAsync(titulo);
            if (!sugestaoResult.IsSuccess) return Result<PayloadAnuncioMLDTO>.Fail(sugestaoResult.Error!);
            categoriaId = sugestaoResult.Value!.CategoriaId;
        }

        var metadadosResult = await _catalogo.ObterMetadadosCategoriaAsync(categoriaId!, accessToken, mercadoLivreUserId);
        if (!metadadosResult.IsSuccess) return Result<PayloadAnuncioMLDTO>.Fail(metadadosResult.Error!);
        var metadados = metadadosResult.Value!;

        var fotos = await ObterFotosAbsolutasAsync(entidadeTipo, entidadeId);

        // Marca/modelo/ano/km/câmbio/combustível/cor — os campos descritivos que
        // o cadastro de veículo realmente tem. "Portas"/"Versão" (citados na
        // tarefa) NÃO existem no domínio hoje — se a categoria exigir, cai no
        // "não mapeado" abaixo e a publicação falha com mensagem clara em vez
        // de inventar um valor.
        var valoresPorSinonimo = new Dictionary<string, string>
        {
            ["MARCA"] = marca,
            ["BRAND"] = marca,
            ["MODELO"] = modelo,
            ["MODEL"] = modelo,
            ["ANO"] = ano.ToString(),
            ["ANO DE FABRICACAO"] = ano.ToString(),
            ["VEHICLE_YEAR"] = ano.ToString(),
            ["QUILOMETRAGEM"] = km.ToString(),
            ["KILOMETERS"] = km.ToString(),
            ["KM"] = km.ToString(),
            ["COR"] = cor,
            ["COLOR"] = cor,
            ["CAMBIO"] = cambioTexto,
            ["TRANSMISSION"] = cambioTexto,
            ["GEARBOX"] = cambioTexto,
            ["COMBUSTIVEL"] = combustivelTexto,
            ["FUEL_TYPE"] = combustivelTexto,
        };

        var resolucao = ResolverAtributosObrigatorios(metadados.AtributosObrigatorios, valoresPorSinonimo);
        if (resolucao.Erros.Count > 0)
            return Result<PayloadAnuncioMLDTO>.Fail(
                $"Não foi possível publicar: {string.Join(" | ", resolucao.Erros)}");

        var payload = new
        {
            title = titulo,
            category_id = categoriaId,
            price = preco,
            currency_id = "BRL",
            available_quantity = 1, // veículo é peça única
            buying_mode = metadados.BuyingMode,
            condition = "used", // fato do domínio (todo veículo aqui já teve IPVA/emplacamento), não metadado do ML
            listing_type_id = metadados.ListingTypeId,
            description = new { plain_text = descricao },
            pictures = fotos.Select(url => new { source = url }).ToList(),
            attributes = resolucao.Atributos
            // Sem bloco "shipping" — classificado não tem envio.
        };

        return Result<PayloadAnuncioMLDTO>.Ok(new PayloadAnuncioMLDTO
        {
            Payload = payload,
            CategoriaId = categoriaId!,
            ListingTypeId = metadados.ListingTypeId,
            Preco = preco
        });
    }

    /// <summary>
    /// Resolve cada atributo obrigatório contra o mapa de valores locais
    /// disponíveis. Quando o atributo tem lista fechada, procura o value_id
    /// batendo o texto local com os nomes da API (normalizado); se não achar e
    /// o atributo não aceitar texto livre, isso vira erro. Quando não há NENHUM
    /// dado local mapeado pro atributo, também vira erro — nos dois casos com
    /// mensagem dizendo exatamente qual atributo e por quê (item 3 da tarefa).
    /// </summary>
    private static (List<object> Atributos, List<string> Erros) ResolverAtributosObrigatorios(
        List<AtributoCategoriaDTO> obrigatorios, Dictionary<string, string> valoresPorSinonimo)
    {
        var atributos = new List<object>();
        var erros = new List<string>();

        foreach (var attr in obrigatorios)
        {
            var valorLocal = BuscarValorLocal(attr, valoresPorSinonimo);
            if (valorLocal is null)
            {
                erros.Add($"o Mercado Livre exige o atributo \"{attr.Nome}\" e o veículo não tem esse dado cadastrado no sistema");
                continue;
            }

            if (attr.AceitaTextoLivre)
            {
                atributos.Add(new { id = attr.Id, value_name = valorLocal });
                continue;
            }

            var valorCatalogo = MercadoLivreCatalogoService.ResolverValorCatalogo(attr, valorLocal);
            if (valorCatalogo is null)
            {
                var opcoes = string.Join(", ", attr.ValoresPermitidos.Select(v => v.Nome));
                erros.Add($"o valor \"{valorLocal}\" cadastrado para \"{attr.Nome}\" não corresponde a nenhuma opção aceita pelo Mercado Livre (opções: {opcoes})");
                continue;
            }

            atributos.Add(new { id = attr.Id, value_id = valorCatalogo.Id });
        }

        return (atributos, erros);
    }

    private static string? BuscarValorLocal(AtributoCategoriaDTO attr, Dictionary<string, string> valoresPorSinonimo)
    {
        var chaveId = MercadoLivreCatalogoService.NormalizarTexto(attr.Id);
        if (valoresPorSinonimo.TryGetValue(chaveId, out var porId)) return porId;

        var chaveNome = MercadoLivreCatalogoService.NormalizarTexto(attr.Nome);
        if (valoresPorSinonimo.TryGetValue(chaveNome, out var porNome)) return porNome;

        foreach (var (sinonimo, valor) in valoresPorSinonimo)
        {
            if (chaveNome.Contains(sinonimo) || chaveId.Contains(sinonimo))
                return valor;
        }

        return null;
    }

    private static string TraduzirCambio(string valorEnum) => valorEnum switch
    {
        "Manual" => "Manual",
        "Automatico" => "Automático",
        _ => valorEnum
    };

    private static string TraduzirCombustivel(string valorEnum) => valorEnum switch
    {
        "Flex" => "Flex",
        "Alcool" => "Álcool",
        "Gasolina" => "Gasolina",
        "Diesel" => "Diesel",
        "Eletrico" => "Elétrico",
        "Hibrido" => "Híbrido",
        _ => valorEnum
    };

    private async Task<List<string>> ObterFotosAbsolutasAsync(string entidadeTipo, Guid entidadeId)
    {
        var r = await _fotoService.GetFotosByEntidadeAsync(entidadeTipo, entidadeId);
        if (!r.IsSuccess || r.Value is null) return new List<string>();

        var baseUrl = _config.UrlBasePublica?.TrimEnd('/') ?? "";
        return r.Value
            .OrderBy(f => f.Ordem)
            .Select(f => f.Url.StartsWith("http") ? f.Url : $"{baseUrl}{f.Url}")
            .ToList();
    }
}
