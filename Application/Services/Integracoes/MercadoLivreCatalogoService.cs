using System.Globalization;
using System.Text;
using System.Text.Json;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Application.Services.Integracoes;

/// <summary>
/// Descobre e cacheia tudo que o Mercado Livre dita sobre uma categoria —
/// nenhum category_id, listing_type_id ou buying_mode fica fixo no código;
/// tudo vem daqui. Consultado pelos construtores de payload
/// (IConstrutorPayloadAnuncio) antes de montar qualquer anúncio.
///
/// Os métodos que leem JsonElement cru fazem isso de propósito: o formato
/// exato das respostas do ML não pôde ser confirmado ao vivo nesta sessão de
/// desenvolvimento (sandbox sem saída de rede para api.mercadolibre.com nem
/// para os docs — ambos bloqueados por policy, HTTP 403 confirmado). O
/// primeiro uso real (fora deste ambiente) deve ser conferido contra o log —
/// cada chamada em MercadoLivreApiClientReal já loga a resposta bruta.
/// </summary>
public class MercadoLivreCatalogoService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(6);

    private readonly IMercadoLivreApiClient _apiClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MercadoLivreCatalogoService> _logger;

    public MercadoLivreCatalogoService(
        IMercadoLivreApiClient apiClient,
        IMemoryCache cache,
        ILogger<MercadoLivreCatalogoService> logger)
    {
        _apiClient = apiClient;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>Sugere a categoria mais provável a partir de um título (GET .../domain_discovery/search). Cacheada por texto.</summary>
    public async Task<Result<SugestaoCategoriaDTO>> SugerirCategoriaAsync(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return Result<SugestaoCategoriaDTO>.Fail("Texto para sugestão de categoria não pode ser vazio.");

        var cacheKey = $"ml:sugestao:{texto.Trim().ToLowerInvariant()}";
        if (_cache.TryGetValue(cacheKey, out SugestaoCategoriaDTO? cached) && cached is not null)
            return Result<SugestaoCategoriaDTO>.Ok(cached);

        JsonElement resposta;
        try
        {
            resposta = await _apiClient.SugerirCategoriaAsync(texto);
        }
        catch (MercadoLivreApiException ex)
        {
            return Result<SugestaoCategoriaDTO>.Fail(ex.Message);
        }

        // Formato esperado (não confirmado ao vivo nesta sessão): array de
        // objetos com "category_id"/"category_name". Ver nota de limitação no
        // relatório final.
        if (resposta.ValueKind != JsonValueKind.Array || resposta.GetArrayLength() == 0)
            return Result<SugestaoCategoriaDTO>.Fail(
                "O Mercado Livre não sugeriu nenhuma categoria para esse texto. Informe a categoria manualmente.");

        var primeiro = resposta[0];
        var categoriaId = LerString(primeiro, "category_id");
        var categoriaNome = LerString(primeiro, "category_name");

        if (string.IsNullOrWhiteSpace(categoriaId))
            return Result<SugestaoCategoriaDTO>.Fail(
                "A resposta do Mercado Livre não trouxe um category_id reconhecível (formato inesperado — confira o log da aplicação). Informe a categoria manualmente.");

        var sugestao = new SugestaoCategoriaDTO { CategoriaId = categoriaId, NomeCategoria = categoriaNome ?? categoriaId };
        _cache.Set(cacheKey, sugestao, Ttl);
        return Result<SugestaoCategoriaDTO>.Ok(sugestao);
    }

    /// <summary>
    /// Método de conveniência: consolida categoria + atributos + modalidade de
    /// anúncio escolhida (a mais barata disponível pra essa conta) num único
    /// objeto. É o que os construtores de payload consultam — nenhum decide
    /// isso sozinho.
    /// </summary>
    public async Task<Result<CategoriaMetadadosDTO>> ObterMetadadosCategoriaAsync(
        string categoriaId, string accessToken, string? mercadoLivreUserId)
    {
        if (string.IsNullOrWhiteSpace(mercadoLivreUserId))
            return Result<CategoriaMetadadosDTO>.Fail(
                "Conta Mercado Livre conectada sem ID de usuário salvo. Reconecte em Integrações > Mercado Livre.");

        var cacheKey = $"ml:categoria:{categoriaId}:{mercadoLivreUserId}";
        if (_cache.TryGetValue(cacheKey, out CategoriaMetadadosDTO? cached) && cached is not null)
        {
            _logger.LogInformation("Metadados da categoria {CategoriaId} servidos do cache.", categoriaId);
            return Result<CategoriaMetadadosDTO>.Ok(cached);
        }

        JsonElement categoria, atributos, modalidades;
        try
        {
            categoria = await _apiClient.ObterCategoriaAsync(categoriaId);
            atributos = await _apiClient.ObterAtributosCategoriaAsync(categoriaId);
            modalidades = await _apiClient.ObterModalidadesAnuncioAsync(categoriaId, mercadoLivreUserId, accessToken);
        }
        catch (MercadoLivreApiException ex)
        {
            return Result<CategoriaMetadadosDTO>.Fail(ex.Message);
        }

        var nome = LerString(categoria, "name") ?? categoriaId;

        var buyingModes = new List<string>();
        if (categoria.TryGetProperty("settings", out var settings) && settings.ValueKind == JsonValueKind.Object
            && settings.TryGetProperty("buying_modes", out var bm) && bm.ValueKind == JsonValueKind.Array)
        {
            buyingModes = bm.EnumerateArray()
                .Select(x => x.GetString() ?? "")
                .Where(x => x != "")
                .ToList();
        }

        var ehClassificado = buyingModes.Any(m => m.Equals("classified", StringComparison.OrdinalIgnoreCase));
        var buyingMode = ehClassificado ? "classified" : (buyingModes.FirstOrDefault() ?? "buy_it_now");

        var listingTypeId = EscolherModalidadeMaisBarata(modalidades);
        if (string.IsNullOrWhiteSpace(listingTypeId))
            return Result<CategoriaMetadadosDTO>.Fail(
                $"Não há nenhuma modalidade de anúncio disponível para a categoria \"{nome}\" nessa conta do Mercado Livre.");

        var atributosObrigatorios = new List<AtributoCategoriaDTO>();
        var atributosOpcionais = new List<AtributoCategoriaDTO>();

        if (atributos.ValueKind == JsonValueKind.Array)
        {
            foreach (var attr in atributos.EnumerateArray())
            {
                var dto = LerAtributo(attr);
                if (dto is null) continue;

                var obrigatorio = attr.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Object
                    && tags.TryGetProperty("required", out var req) && req.ValueKind == JsonValueKind.True;

                (obrigatorio ? atributosObrigatorios : atributosOpcionais).Add(dto);
            }
        }

        var resultado = new CategoriaMetadadosDTO
        {
            CategoriaId = categoriaId,
            NomeCategoria = nome,
            EhClassificado = ehClassificado,
            BuyingMode = buyingMode,
            ListingTypeId = listingTypeId,
            AtributosObrigatorios = atributosObrigatorios,
            AtributosOpcionais = atributosOpcionais
        };

        _cache.Set(cacheKey, resultado, Ttl);
        return Result<CategoriaMetadadosDTO>.Ok(resultado);
    }

    /// <summary>
    /// Compara um valor local com os nomes das opções aceitas pelo atributo,
    /// normalizando caixa e acentos (ex.: "Automático" casa com "AUTOMATICO").
    /// Devolve null quando não achou correspondência — quem chama decide se
    /// aborta ou tenta texto livre.
    /// </summary>
    public static ValorCatalogoDTO? ResolverValorCatalogo(AtributoCategoriaDTO atributo, string valorLocal)
    {
        var alvo = NormalizarTexto(valorLocal);

        return atributo.ValoresPermitidos.FirstOrDefault(v => NormalizarTexto(v.Nome) == alvo)
            ?? atributo.ValoresPermitidos.FirstOrDefault(v =>
                NormalizarTexto(v.Nome).Contains(alvo) || alvo.Contains(NormalizarTexto(v.Nome)));
    }

    public static string NormalizarTexto(string texto)
    {
        var normalizado = texto.Trim().Normalize(NormalizationForm.FormD);
        var semAcento = new string(normalizado
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        return semAcento.ToUpperInvariant();
    }

    /// <summary>
    /// Escolhe a modalidade mais barata entre as disponíveis (gratuita, se
    /// houver) — é ambiente de teste, não faz sentido gastar exposição paga.
    /// A ordem de preferência abaixo é heurística (nomes de modalidade
    /// conhecidos do ML, do mais barato pro mais caro) e NÃO foi confirmada
    /// contra uma tabela de preços real nesta sessão — se a modalidade
    /// devolvida pela API não estiver nesta lista, cai no primeiro item que a
    /// própria API devolveu (a ordem de retorno costuma já refletir a
    /// preferência do ML).
    /// </summary>
    private static string EscolherModalidadeMaisBarata(JsonElement modalidades)
    {
        var ids = new List<string>();

        if (modalidades.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in modalidades.EnumerateArray())
            {
                string? id = item.ValueKind switch
                {
                    JsonValueKind.String => item.GetString(),
                    JsonValueKind.Object when item.TryGetProperty("id", out var idProp) => idProp.GetString(),
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(id))
                    ids.Add(id);
            }
        }

        if (ids.Count == 0) return "";

        var ordemPreferencia = new[] { "free", "bronze", "silver", "gold_pro", "gold_special", "gold_premium", "gold" };
        foreach (var pref in ordemPreferencia)
        {
            var achado = ids.FirstOrDefault(id => id.Equals(pref, StringComparison.OrdinalIgnoreCase));
            if (achado is not null) return achado;
        }

        return ids[0];
    }

    private static AtributoCategoriaDTO? LerAtributo(JsonElement attr)
    {
        var id = LerString(attr, "id");
        if (string.IsNullOrWhiteSpace(id)) return null;

        var nome = LerString(attr, "name");
        var valueType = LerString(attr, "value_type");

        var valores = new List<ValorCatalogoDTO>();
        if (attr.TryGetProperty("values", out var vals) && vals.ValueKind == JsonValueKind.Array)
        {
            foreach (var v in vals.EnumerateArray())
            {
                var vid = LerString(v, "id");
                var vnome = LerString(v, "name");
                if (!string.IsNullOrWhiteSpace(vid) && !string.IsNullOrWhiteSpace(vnome))
                    valores.Add(new ValorCatalogoDTO { Id = vid, Nome = vnome });
            }
        }

        // "list" com valores fechados => precisa de value_id; qualquer outro
        // tipo (string, number...) ou lista sem values fechados => texto livre.
        var aceitaTextoLivre = valueType != "list" || valores.Count == 0;

        return new AtributoCategoriaDTO
        {
            Id = id,
            Nome = nome ?? id,
            AceitaTextoLivre = aceitaTextoLivre,
            ValoresPermitidos = valores
        };
    }

    private static string? LerString(JsonElement el, string prop)
        => el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;
}
