using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Infrastructure.Services;

/// <summary>
/// Cliente HTTP real para a API do Mercado Livre — única implementação de
/// IMercadoLivreApiClient (registrada incondicionalmente na DI). Código
/// completo, mas NÃO testável de ponta a ponta neste ambiente de
/// desenvolvimento: OAuth redirect e entrega de webhook exigem um host HTTPS
/// público alcançável pelo Mercado Livre, que não existe aqui.
///
/// NOTA: não foi possível confirmar nesta sessão (docs oficiais bloquearam fetch
/// automatizado) se o Sandbox do ML usa um host de API separado ou o mesmo host de
/// produção com credenciais de usuário de teste. Implementado como o segundo caso
/// (mesma BaseAddress) — MercadoLivreConfig.Ambiente é hoje só metadado/exibição.
/// Confirmar contra a documentação atual do ML antes de usar Sandbox de verdade.
/// </summary>
public class MercadoLivreApiClientReal : IMercadoLivreApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MercadoLivreConfig _config;
    private readonly ILogger<MercadoLivreApiClientReal> _logger;

    public MercadoLivreApiClientReal(
        HttpClient httpClient,
        IOptions<MercadoLivreConfig> config,
        ILogger<MercadoLivreApiClientReal> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<MercadoLivreTokenResponseDTO> TrocarCodigoPorTokenAsync(string code)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = _config.RedirectUri
        };

        var response = await _httpClient.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));
        await GarantirSucessoAsync(response, "trocar código OAuth por token");
        var resultado = await response.Content.ReadFromJsonAsync<MercadoLivreTokenResponseDTO>()
            ?? throw new InvalidOperationException("Resposta vazia ao trocar código por token.");

        _logger.LogInformation("Token OAuth obtido para usuário ML {UserId}", resultado.UserId);
        return resultado;
    }

    public async Task<MercadoLivreTokenResponseDTO> RenovarTokenAsync(string refreshToken)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret,
            ["refresh_token"] = refreshToken
        };

        var response = await _httpClient.PostAsync("/oauth/token", new FormUrlEncodedContent(payload));
        await GarantirSucessoAsync(response, "renovar token OAuth");
        var resultado = await response.Content.ReadFromJsonAsync<MercadoLivreTokenResponseDTO>()
            ?? throw new InvalidOperationException("Resposta vazia ao renovar token.");

        _logger.LogInformation("Token OAuth renovado para usuário ML {UserId}", resultado.UserId);
        return resultado;
    }

    public async Task<MercadoLivreUsuarioDTO> ObterUsuarioAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, "obter usuário");
        return await response.Content.ReadFromJsonAsync<MercadoLivreUsuarioDTO>()
            ?? throw new InvalidOperationException("Resposta vazia ao obter usuário.");
    }

    public async Task<string> PublicarItemAsync(object payload, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/items") { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, "publicar item");

        var resultado = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var itemId = resultado?["id"]?.ToString() ?? throw new InvalidOperationException("Resposta sem id ao publicar item.");

        _logger.LogInformation("Item publicado no ML -> {ItemId}", itemId);
        return itemId;
    }

    public async Task AtualizarItemAsync(string itemIdML, MercadoLivreItemDTO item, string accessToken)
    {
        var payload = new { available_quantity = item.Quantidade, price = item.Preco };

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/items/{itemIdML}") { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, $"atualizar item {itemIdML}");
        _logger.LogInformation("Item {ItemId} atualizado no ML — Qtd: {Quantidade}, Preço: {Preco}", itemIdML, item.Quantidade, item.Preco);
    }

    public Task PausarItemAsync(string itemIdML, string accessToken)
        => AtualizarStatusItemAsync(itemIdML, "paused", accessToken);

    public Task EncerrarItemAsync(string itemIdML, string accessToken)
        => AtualizarStatusItemAsync(itemIdML, "closed", accessToken);

    private async Task AtualizarStatusItemAsync(string itemIdML, string status, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/items/{itemIdML}")
        {
            Content = JsonContent.Create(new { status })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, $"alterar status do item {itemIdML} para {status}");
        _logger.LogInformation("Item {ItemId} -> status {Status} no ML", itemIdML, status);
    }

    public async Task<MercadoLivrePedidoDTO> ObterPedidoAsync(string resourceUrl, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, resourceUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, $"obter pedido {resourceUrl}");
        return await response.Content.ReadFromJsonAsync<MercadoLivrePedidoDTO>()
            ?? throw new InvalidOperationException("Resposta vazia ao obter pedido.");
    }

    // === Catálogo ===
    //
    // Os quatro métodos abaixo devolvem JsonElement cru (em vez de DTO tipado)
    // de propósito: o formato exato de cada resposta não pôde ser confirmado
    // contra a API real nesta sessão de desenvolvimento (ambiente sandbox sem
    // saída de rede para api.mercadolibre.com nem para os docs do ML — ambos
    // bloqueados por policy, confirmado por HTTP 403 em teste direto). Manter
    // o JSON cru permite ao MercadoLivreCatalogoService extrair campos de forma
    // defensiva (TryGetProperty) e logar a resposta inteira na primeira
    // chamada real — é assim que se confirma o formato certo em vez de travar
    // o app inteiro numa suposição errada de nome de campo.

    public async Task<JsonElement> SugerirCategoriaAsync(string texto)
    {
        // domain_discovery/search é público (não exige Authorization) — é o
        // mesmo endpoint que a busca do próprio site do ML usa para sugerir
        // categoria a partir de um texto livre.
        var q = Uri.EscapeDataString(texto);
        var response = await _httpClient.GetAsync($"/sites/MLB/domain_discovery/search?q={q}&limit=1");
        await GarantirSucessoAsync(response, $"sugerir categoria para \"{texto}\"");

        var corpo = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("domain_discovery/search(\"{Texto}\") -> {Corpo}", texto, corpo);

        using var doc = JsonDocument.Parse(corpo);
        return doc.RootElement.Clone();
    }

    public async Task<JsonElement> ObterCategoriaAsync(string categoriaId)
    {
        var response = await _httpClient.GetAsync($"/categories/{categoriaId}");
        await GarantirSucessoAsync(response, $"obter categoria {categoriaId}");

        var corpo = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("categories/{CategoriaId} -> {Corpo}", categoriaId, corpo);

        using var doc = JsonDocument.Parse(corpo);
        return doc.RootElement.Clone();
    }

    public async Task<JsonElement> ObterAtributosCategoriaAsync(string categoriaId)
    {
        var response = await _httpClient.GetAsync($"/categories/{categoriaId}/attributes");
        await GarantirSucessoAsync(response, $"obter atributos da categoria {categoriaId}");

        var corpo = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("categories/{CategoriaId}/attributes -> {Corpo}", categoriaId, corpo);

        using var doc = JsonDocument.Parse(corpo);
        return doc.RootElement.Clone();
    }

    public async Task<JsonElement> ObterModalidadesAnuncioAsync(string categoriaId, string userId, string accessToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"/users/{userId}/available_listing_types?category_id={categoriaId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, $"obter modalidades de anúncio da categoria {categoriaId}");

        var corpo = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("users/{UserId}/available_listing_types?category_id={CategoriaId} -> {Corpo}", userId, categoriaId, corpo);

        using var doc = JsonDocument.Parse(corpo);
        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Lança com o corpo da resposta de erro anexado — a mensagem crua do
    /// EnsureSuccessStatusCode() (só o status code) não é suficiente para
    /// diagnosticar rejeições da API do ML (ex.: token expirado, categoria
    /// inválida, permissão faltando), que sempre vêm com um corpo JSON explicando o motivo.
    /// </summary>
    private async Task GarantirSucessoAsync(HttpResponseMessage response, string operacao)
    {
        if (response.IsSuccessStatusCode) return;

        var corpo = await response.Content.ReadAsStringAsync();

        // Log sempre com o corpo COMPLETO, sem truncar — é o que permite
        // diagnosticar depois, mesmo que a mensagem traduzida na exceção seja
        // resumida.
        _logger.LogError(
            "Falha ao {Operacao} — HTTP {Status}: {Corpo}",
            operacao, (int)response.StatusCode, corpo);

        throw new MercadoLivreApiException(TraduzirErro(corpo, operacao, (int)response.StatusCode), corpo);
    }

    /// <summary>
    /// Traduz o corpo de erro do ML ({"message","error","cause":[...]}) para uma
    /// mensagem em português, cobrindo os casos mais comuns de rejeição de
    /// publicação. Nunca falha: se a desserialização não bater com o formato
    /// esperado (formato não confirmado contra a API real nesta sessão), cai no
    /// fallback com a mensagem/corpo cru — o operador nunca fica sem nenhuma
    /// pista do que aconteceu.
    /// </summary>
    private static string TraduzirErro(string corpoJson, string operacao, int statusCode)
    {
        MercadoLivreErroDTO? erro;
        try
        {
            erro = JsonSerializer.Deserialize<MercadoLivreErroDTO>(
                corpoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            erro = null;
        }

        if (erro is null)
            return $"Mercado Livre recusou a operação \"{operacao}\" (HTTP {statusCode}): {corpoJson}";

        var codigo = erro.Error ?? "";

        if (codigo.Contains("listing_type", StringComparison.OrdinalIgnoreCase))
            return $"A modalidade de anúncio escolhida não é aceita para essa categoria no Mercado Livre. Detalhe: {erro.Message}";

        if (codigo.Contains("category", StringComparison.OrdinalIgnoreCase) || codigo.Contains("category_id", StringComparison.OrdinalIgnoreCase))
            return $"A categoria informada é inválida no Mercado Livre. Detalhe: {erro.Message}";

        if (codigo.Contains("token", StringComparison.OrdinalIgnoreCase) || statusCode == 401)
            return "O token de acesso ao Mercado Livre expirou ou é inválido. Reconecte em Integrações > Mercado Livre.";

        if (codigo.Contains("picture", StringComparison.OrdinalIgnoreCase))
            return $"O Mercado Livre não conseguiu acessar uma ou mais imagens enviadas — confira se a URL pública do sistema está correta e acessível pela internet. Detalhe: {erro.Message}";

        var atributosFaltando = erro.Cause
            .Where(c => (c.Code ?? c.Message ?? "").Contains("required", StringComparison.OrdinalIgnoreCase)
                     || (c.Code ?? c.Message ?? "").Contains("attribute", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Message ?? c.Code)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        if (atributosFaltando.Count > 0)
            return $"O Mercado Livre recusou a publicação por atributo(s) obrigatório(s) ausente(s): {string.Join("; ", atributosFaltando)}";

        var causas = erro.Cause.Select(c => c.Message ?? c.Code).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
        var detalhe = causas.Count > 0 ? string.Join("; ", causas) : erro.Message;

        return $"Mercado Livre recusou a operação \"{operacao}\": {detalhe}";
    }
}
