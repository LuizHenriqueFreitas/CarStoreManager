using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Integracoes;
using CarStoreManager.Domain.Entities.Integracoes;
using CarStoreManager.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Application.Services.Integracoes;

public class MercadoLivrePublicacaoService : IMercadoLivrePublicacaoService
{
    private readonly IAnuncioMercadoLivreRepository _anuncioRepo;
    private readonly IVeiculoVendaRepository _veiculoVendaRepo;
    private readonly IComponenteRepository _componenteRepo;
    private readonly IEstoqueRepository _estoqueRepo;
    private readonly IVeiculoConsignacaoRepository _veiculoConsignacaoRepo;
    private readonly IFotoService _fotoService;
    private readonly IMercadoLivreApiClient _apiClient;
    private readonly MercadoLivreTokenHelper _tokenHelper;
    private readonly MercadoLivreConfig _config;

    public MercadoLivrePublicacaoService(
        IAnuncioMercadoLivreRepository anuncioRepo,
        IVeiculoVendaRepository veiculoVendaRepo,
        IComponenteRepository componenteRepo,
        IEstoqueRepository estoqueRepo,
        IVeiculoConsignacaoRepository veiculoConsignacaoRepo,
        IFotoService fotoService,
        IMercadoLivreApiClient apiClient,
        MercadoLivreTokenHelper tokenHelper,
        IOptions<MercadoLivreConfig> config)
    {
        _anuncioRepo = anuncioRepo;
        _veiculoVendaRepo = veiculoVendaRepo;
        _componenteRepo = componenteRepo;
        _estoqueRepo = estoqueRepo;
        _veiculoConsignacaoRepo = veiculoConsignacaoRepo;
        _fotoService = fotoService;
        _apiClient = apiClient;
        _tokenHelper = tokenHelper;
        _config = config.Value;
    }

    public async Task<Result<string>> PublicarAsync(PublicarAnuncioDTO dto)
    {
        var tokenResult = await _tokenHelper.ObterAccessTokenValidoAsync();
        if (!tokenResult.IsSuccess)
            return Result<string>.Fail(tokenResult.Error!);

        var itemResult = await MontarItemAsync(dto.EntidadeTipo, dto.EntidadeId);
        if (!itemResult.IsSuccess)
            return Result<string>.Fail(itemResult.Error!);

        try
        {
            var itemIdML = await _apiClient.PublicarItemAsync(itemResult.Value!, tokenResult.Value!);

            var anuncioExistente = await _anuncioRepo.ObterPorEntidadeAsync(dto.EntidadeTipo, dto.EntidadeId);
            var anuncio = anuncioExistente ?? new AnuncioMercadoLivre(dto.EntidadeTipo, dto.EntidadeId);
            anuncio.MarcarComoPublicado(itemIdML, itemResult.Value!.Preco);

            if (anuncioExistente is null)
                await _anuncioRepo.AddAsync(anuncio);
            else
                _anuncioRepo.Update(anuncio);
            await _anuncioRepo.SaveChangesAsync();

            return Result<string>.Ok(itemIdML);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Erro ao publicar no Mercado Livre: {ex.Message}");
        }
    }

    public async Task<Result> PausarAsync(Guid anuncioId)
        => await ExecutarAcaoAnuncioAsync(anuncioId,
            (client, itemId, token) => client.PausarItemAsync(itemId, token),
            anuncio => anuncio.MarcarComoPausado());

    public async Task<Result> EncerrarAsync(Guid anuncioId)
        => await ExecutarAcaoAnuncioAsync(anuncioId,
            (client, itemId, token) => client.EncerrarItemAsync(itemId, token),
            anuncio => anuncio.MarcarComoEncerrado());

    public async Task<Result<List<AnuncioMercadoLivreDTO>>> ListarAsync()
    {
        var anuncios = await _anuncioRepo.GetAllAsync();
        var lista = new List<AnuncioMercadoLivreDTO>();

        foreach (var a in anuncios)
        {
            var nome = await ObterNomeEntidadeAsync(a.EntidadeTipo, a.EntidadeId);
            lista.Add(AnuncioMercadoLivreMapping.ToDto(a, nome));
        }

        return Result<List<AnuncioMercadoLivreDTO>>.Ok(lista);
    }

    private async Task<Result> ExecutarAcaoAnuncioAsync(
        Guid anuncioId,
        Func<IMercadoLivreApiClient, string, string, Task> chamarApi,
        Action<AnuncioMercadoLivre> aplicarLocalmente)
    {
        var anuncio = await _anuncioRepo.GetByIdAsync(anuncioId);
        if (anuncio is null)
            return Result.Fail("Anúncio não encontrado");

        var tokenResult = await _tokenHelper.ObterAccessTokenValidoAsync();
        if (!tokenResult.IsSuccess)
            return Result.Fail(tokenResult.Error!);

        try
        {
            await chamarApi(_apiClient, anuncio.ItemIdML, tokenResult.Value!);
            aplicarLocalmente(anuncio);
            _anuncioRepo.Update(anuncio);
            await _anuncioRepo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    private async Task<Result<MercadoLivreItemDTO>> MontarItemAsync(string entidadeTipo, Guid entidadeId)
    {
        switch (entidadeTipo)
        {
            case "VeiculoVenda":
            {
                var veiculo = await _veiculoVendaRepo.GetByIdAsync(entidadeId);
                if (veiculo is null) return Result<MercadoLivreItemDTO>.Fail("Veículo não encontrado");

                var fotos = await ObterFotosAbsolutasAsync("VeiculoVenda", entidadeId);
                return Result<MercadoLivreItemDTO>.Ok(new MercadoLivreItemDTO
                {
                    Titulo = $"{veiculo.GetMarca()} {veiculo.GetModelo()} {veiculo.GetAno()}",
                    Preco = veiculo.GetValor(),
                    Quantidade = 1,
                    Descricao = string.IsNullOrWhiteSpace(veiculo.TextoTermoPreliminar)
                        ? "Veículo em perfeitas condições."
                        : veiculo.TextoTermoPreliminar,
                    UrlsFotos = fotos,
                    CategoriaML = "MLB1744", // placeholder — categoria real exige lookup na API de categorias do ML
                    BuyingMode = "classified", // veículos são categoria de classificados no ML
                    Condicao = "used",
                    Atributos = new Dictionary<string, string>
                    {
                        ["BRAND"] = veiculo.GetMarca(),
                        ["MODEL"] = veiculo.GetModelo(),
                        ["VEHICLE_YEAR"] = veiculo.GetAno().ToString(),
                        ["KILOMETERS"] = veiculo.GetQuilometragem().ToString()
                    }
                });
            }

            case "Componente":
            {
                var componente = await _componenteRepo.GetByIdAsync(entidadeId);
                if (componente is null) return Result<MercadoLivreItemDTO>.Fail("Componente não encontrado");

                var estoque = await _estoqueRepo.ObterPorComponenteAsync(entidadeId);
                if (estoque is null || estoque.QuantidadeAtual <= 0)
                    return Result<MercadoLivreItemDTO>.Fail("Componente sem estoque disponível");

                return Result<MercadoLivreItemDTO>.Ok(new MercadoLivreItemDTO
                {
                    Titulo = componente.GetNome(),
                    Preco = componente.ValorVenda,
                    Quantidade = estoque.QuantidadeAtual,
                    Descricao = componente.GetDescricao(),
                    UrlsFotos = new List<string>(), // Componente não tem infraestrutura de fotos hoje
                    CategoriaML = "MLB1747", // placeholder
                    Condicao = "new"
                });
            }

            case "VeiculoConsignacao":
            {
                var veiculo = await _veiculoConsignacaoRepo.GetByIdAsync(entidadeId);
                if (veiculo is null) return Result<MercadoLivreItemDTO>.Fail("Veículo consignado não encontrado");

                var fotos = await ObterFotosAbsolutasAsync("VeiculoConsignacao", entidadeId);
                return Result<MercadoLivreItemDTO>.Ok(new MercadoLivreItemDTO
                {
                    Titulo = $"{veiculo.GetMarca()} {veiculo.GetModelo()} {veiculo.GetAno()}",
                    Preco = veiculo.Comissao.ValorVendaEsperado.GetValorDinheiro(),
                    Quantidade = 1,
                    Descricao = string.IsNullOrWhiteSpace(veiculo.TextoContrato)
                        ? "Veículo consignado em perfeitas condições."
                        : veiculo.TextoContrato,
                    UrlsFotos = fotos,
                    CategoriaML = "MLB1744", // placeholder
                    BuyingMode = "classified", // veículos são categoria de classificados no ML
                    Condicao = "used",
                    Atributos = new Dictionary<string, string>
                    {
                        ["BRAND"] = veiculo.GetMarca(),
                        ["MODEL"] = veiculo.GetModelo(),
                        ["VEHICLE_YEAR"] = veiculo.GetAno().ToString(),
                        ["KILOMETERS"] = veiculo.GetQuilometragem().ToString()
                    }
                });
            }

            default:
                return Result<MercadoLivreItemDTO>.Fail($"Tipo de entidade não suportado: {entidadeTipo}");
        }
    }

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

    private async Task<string> ObterNomeEntidadeAsync(string entidadeTipo, Guid entidadeId)
    {
        switch (entidadeTipo)
        {
            case "VeiculoVenda":
                var v = await _veiculoVendaRepo.GetByIdAsync(entidadeId);
                return v is null ? "(removido)" : $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()}";
            case "Componente":
                var c = await _componenteRepo.GetByIdAsync(entidadeId);
                return c?.GetNome() ?? "(removido)";
            case "VeiculoConsignacao":
                var vc = await _veiculoConsignacaoRepo.GetByIdAsync(entidadeId);
                return vc is null ? "(removido)" : $"{vc.GetMarca()} {vc.GetModelo()} {vc.GetAno()}";
            default:
                return "(desconhecido)";
        }
    }
}
