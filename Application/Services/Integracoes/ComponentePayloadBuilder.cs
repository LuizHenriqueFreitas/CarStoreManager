using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Integracoes;

/// <summary>
/// Monta o payload de publicação para Componente — produto comum de
/// e-commerce (não classificado), fluxo prioritário da tarefa: é o que fecha
/// o ciclo completo (anúncio -> venda -> webhook -> baixa de estoque). Mais
/// simples que o de veículo porque autopeças aceitam buying_mode/listing_type
/// direto do que a categoria oferecer, sem a complicação de "classificado".
/// </summary>
public class ComponentePayloadBuilder : IConstrutorPayloadAnuncio
{
    private readonly IComponenteRepository _componenteRepo;
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly MercadoLivreCatalogoService _catalogo;

    public ComponentePayloadBuilder(
        IComponenteRepository componenteRepo,
        IEstoqueRepository estoqueRepo,
        MercadoLivreCatalogoService catalogo)
    {
        _componenteRepo = componenteRepo;
        _estoqueRepo = estoqueRepo;
        _catalogo = catalogo;
    }

    public bool Aceita(string entidadeTipo) => entidadeTipo == "Componente";

    public async Task<Result<PayloadAnuncioMLDTO>> ConstruirAsync(
        string entidadeTipo, Guid entidadeId, string? categoriaOverride, string accessToken, string? mercadoLivreUserId)
    {
        var componente = await _componenteRepo.GetByIdAsync(entidadeId);
        if (componente is null) return Result<PayloadAnuncioMLDTO>.Fail("Componente não encontrado");

        var estoque = await _estoqueRepo.ObterPorComponenteAsync(entidadeId);
        if (estoque is null || estoque.QuantidadeAtual <= 0)
            return Result<PayloadAnuncioMLDTO>.Fail("Componente sem estoque disponível");

        var titulo = componente.GetNome();

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

        // Marca do fabricante, part number e código OEM — os dados descritivos
        // que Componente realmente tem. Garantia entra como atributo (quando a
        // categoria pedir um) formatada em dias, não como campo top-level
        // "warranty": não há confirmação ao vivo nesta sessão do formato exato
        // desse campo na API real (ver relatório final).
        var valoresPorSinonimo = new Dictionary<string, string>
        {
            ["MARCA"] = componente.GetMarcaFabricante(),
            ["BRAND"] = componente.GetMarcaFabricante(),
            ["PART_NUMBER"] = componente.GetPartNumber(),
            ["NUMERO DE PECA"] = componente.GetPartNumber(),
            ["MPN"] = componente.GetPartNumber(),
        };
        if (!string.IsNullOrWhiteSpace(componente.GetCodigoOEM()))
        {
            valoresPorSinonimo["OEM"] = componente.GetCodigoOEM();
            valoresPorSinonimo["CODIGO OEM"] = componente.GetCodigoOEM();
        }
        if (componente.GetGarantiaDias() > 0)
        {
            var garantiaTexto = $"{componente.GetGarantiaDias()} dias";
            valoresPorSinonimo["GARANTIA"] = garantiaTexto;
            valoresPorSinonimo["WARRANTY"] = garantiaTexto;
            valoresPorSinonimo["GARANTIA DO VENDEDOR"] = garantiaTexto;
        }

        var resolucao = ResolverAtributosObrigatorios(metadados.AtributosObrigatorios, valoresPorSinonimo);
        if (resolucao.Erros.Count > 0)
            return Result<PayloadAnuncioMLDTO>.Fail(
                $"Não foi possível publicar: {string.Join(" | ", resolucao.Erros)}");

        var payload = new
        {
            title = titulo,
            category_id = categoriaId,
            price = componente.ValorVenda,
            currency_id = "BRL",
            available_quantity = estoque.QuantidadeAtual,
            buying_mode = metadados.BuyingMode,
            condition = "new",
            listing_type_id = metadados.ListingTypeId,
            description = new { plain_text = componente.GetDescricao() },
            pictures = Array.Empty<object>(), // Componente não tem infraestrutura de fotos hoje — ver item 6 da tarefa
            attributes = resolucao.Atributos
        };

        return Result<PayloadAnuncioMLDTO>.Ok(new PayloadAnuncioMLDTO
        {
            Payload = payload,
            CategoriaId = categoriaId!,
            ListingTypeId = metadados.ListingTypeId,
            Preco = componente.ValorVenda
        });
    }

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
                erros.Add($"o Mercado Livre exige o atributo \"{attr.Nome}\" e o componente não tem esse dado cadastrado no sistema");
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
}
