using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Integracoes;
using CarStoreManager.Domain.Entities.Integracoes;
using CarStoreManager.Domain.Interfaces.Repositories.Integracoes;
using CarStoreManager.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Application.Services.Integracoes;

public class MercadoLivrePublicacaoService : IMercadoLivrePublicacaoService
{
    private readonly IAnuncioMercadoLivreRepository _anuncioRepo;
    private readonly IVeiculoVendaRepository _veiculoVendaRepo;
    private readonly IComponenteRepository _componenteRepo;
    private readonly IVeiculoConsignacaoRepository _veiculoConsignacaoRepo;
    private readonly IConfiguracaoMercadoLivreRepository _configMLRepo;
    private readonly IMercadoLivreApiClient _apiClient;
    private readonly MercadoLivreTokenHelper _tokenHelper;
    private readonly MercadoLivreCatalogoService _catalogo;
    private readonly IEnumerable<IConstrutorPayloadAnuncio> _construtores;
    private readonly MercadoLivreConfig _config;

    public MercadoLivrePublicacaoService(
        IAnuncioMercadoLivreRepository anuncioRepo,
        IVeiculoVendaRepository veiculoVendaRepo,
        IComponenteRepository componenteRepo,
        IVeiculoConsignacaoRepository veiculoConsignacaoRepo,
        IConfiguracaoMercadoLivreRepository configMLRepo,
        IMercadoLivreApiClient apiClient,
        MercadoLivreTokenHelper tokenHelper,
        MercadoLivreCatalogoService catalogo,
        IEnumerable<IConstrutorPayloadAnuncio> construtores,
        IOptions<MercadoLivreConfig> config)
    {
        _anuncioRepo = anuncioRepo;
        _veiculoVendaRepo = veiculoVendaRepo;
        _componenteRepo = componenteRepo;
        _veiculoConsignacaoRepo = veiculoConsignacaoRepo;
        _configMLRepo = configMLRepo;
        _apiClient = apiClient;
        _tokenHelper = tokenHelper;
        _catalogo = catalogo;
        _construtores = construtores;
        _config = config.Value;
    }

    public async Task<Result<string>> PublicarAsync(PublicarAnuncioDTO dto)
    {
        var tokenResult = await _tokenHelper.ObterAccessTokenValidoAsync();
        if (!tokenResult.IsSuccess)
            return Result<string>.Fail(tokenResult.Error!);

        // Item 6: o ML baixa as fotos a partir da URL enviada — localhost não
        // serve. Componente ainda não manda foto (ver builder), então só
        // barra aqui os tipos que dependem de foto pública.
        if (dto.EntidadeTipo != "Componente")
        {
            var urlCheck = ValidarUrlPublica();
            if (!urlCheck.IsSuccess) return Result<string>.Fail(urlCheck.Error!);
        }

        var builder = _construtores.FirstOrDefault(b => b.Aceita(dto.EntidadeTipo));
        if (builder is null)
            return Result<string>.Fail($"Tipo de entidade não suportado: {dto.EntidadeTipo}");

        var cfgML = await _configMLRepo.ObterAsync();

        var payloadResult = await builder.ConstruirAsync(
            dto.EntidadeTipo, dto.EntidadeId, dto.CategoriaMLOverride, tokenResult.Value!, cfgML.MercadoLivreUserId);

        if (!payloadResult.IsSuccess)
        {
            await RegistrarErroAsync(dto, payloadResult.Error!, null);
            return Result<string>.Fail(payloadResult.Error!);
        }

        try
        {
            var itemIdML = await _apiClient.PublicarItemAsync(payloadResult.Value!.Payload, tokenResult.Value!);

            var anuncioExistente = await _anuncioRepo.ObterPorEntidadeAsync(dto.EntidadeTipo, dto.EntidadeId);
            var anuncio = anuncioExistente ?? new AnuncioMercadoLivre(dto.EntidadeTipo, dto.EntidadeId);
            anuncio.MarcarComoPublicado(
                itemIdML, payloadResult.Value.Preco, payloadResult.Value.CategoriaId, payloadResult.Value.ListingTypeId);

            if (anuncioExistente is null)
                await _anuncioRepo.AddAsync(anuncio);
            else
                _anuncioRepo.Update(anuncio);
            await _anuncioRepo.SaveChangesAsync();

            return Result<string>.Ok(itemIdML);
        }
        catch (MercadoLivreApiException mlEx)
        {
            await RegistrarErroAsync(dto, mlEx.Message, mlEx.DetalheTecnico);
            return Result<string>.Fail(mlEx.Message);
        }
        catch (Exception ex)
        {
            var mensagem = $"Erro ao publicar no Mercado Livre: {ex.Message}";
            await RegistrarErroAsync(dto, mensagem, null);
            return Result<string>.Fail(mensagem);
        }
    }

    public async Task<Result<SugestaoCategoriaDTO>> SugerirCategoriaAsync(string entidadeTipo, Guid entidadeId)
    {
        var titulo = await ObterNomeEntidadeAsync(entidadeTipo, entidadeId);
        if (titulo is null || titulo == "(removido)")
            return Result<SugestaoCategoriaDTO>.Fail("Entidade não encontrada");

        return await _catalogo.SugerirCategoriaAsync(titulo);
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
            lista.Add(AnuncioMercadoLivreMapping.ToDto(a, nome ?? "(removido)"));
        }

        return Result<List<AnuncioMercadoLivreDTO>>.Ok(lista);
    }

    private async Task RegistrarErroAsync(PublicarAnuncioDTO dto, string mensagem, string? detalheTecnico)
    {
        var anuncioExistente = await _anuncioRepo.ObterPorEntidadeAsync(dto.EntidadeTipo, dto.EntidadeId);
        var anuncio = anuncioExistente ?? new AnuncioMercadoLivre(dto.EntidadeTipo, dto.EntidadeId);
        anuncio.RegistrarErro(mensagem, detalheTecnico);

        if (anuncioExistente is null)
            await _anuncioRepo.AddAsync(anuncio);
        else
            _anuncioRepo.Update(anuncio);
        await _anuncioRepo.SaveChangesAsync();
    }

    /// <summary>
    /// Item 6: sem host público válido, nem adianta chamar a API — o ML vai
    /// tentar baixar as fotos e falhar de um jeito confuso de diagnosticar.
    /// Aborta localmente com mensagem clara em vez disso.
    /// </summary>
    private Result ValidarUrlPublica()
    {
        var url = (_config.UrlBasePublica ?? "").Trim();

        if (string.IsNullOrWhiteSpace(url))
            return Result.Fail(
                "Publicação cancelada: configure a URL pública do sistema (MercadoLivre:UrlBasePublica no appsettings) antes de publicar — o Mercado Livre precisa baixar as fotos de um endereço acessível pela internet.");

        if (url.Contains("localhost", StringComparison.OrdinalIgnoreCase) || url.Contains("127.0.0.1"))
            return Result.Fail(
                $"Publicação cancelada: a URL pública configurada (\"{url}\") aponta para localhost — o Mercado Livre não consegue baixar fotos desse endereço de fora. Configure um host público (ex.: um túnel ngrok) antes de publicar.");

        return Result.Ok();
    }

    private async Task<string?> ObterNomeEntidadeAsync(string entidadeTipo, Guid entidadeId)
    {
        switch (entidadeTipo)
        {
            case "VeiculoVenda":
                var v = await _veiculoVendaRepo.GetByIdAsync(entidadeId);
                return v is null ? null : $"{v.GetMarca()} {v.GetModelo()} {v.GetAno()}";
            case "Componente":
                var c = await _componenteRepo.GetByIdAsync(entidadeId);
                return c?.GetNome();
            case "VeiculoConsignacao":
                var vc = await _veiculoConsignacaoRepo.GetByIdAsync(entidadeId);
                return vc is null ? null : $"{vc.GetMarca()} {vc.GetModelo()} {vc.GetAno()}";
            default:
                return null;
        }
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
        catch (MercadoLivreApiException mlEx)
        {
            anuncio.RegistrarErro(mlEx.Message, mlEx.DetalheTecnico);
            _anuncioRepo.Update(anuncio);
            await _anuncioRepo.SaveChangesAsync();
            return Result.Fail(mlEx.Message);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}
