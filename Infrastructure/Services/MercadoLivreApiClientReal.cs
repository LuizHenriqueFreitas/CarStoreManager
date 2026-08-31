using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    public async Task<string> PublicarItemAsync(MercadoLivreItemDTO item, string accessToken)
    {
        var payload = new
        {
            title = item.Titulo,
            category_id = string.IsNullOrWhiteSpace(item.CategoriaML) ? "MLB1234" : item.CategoriaML,
            price = item.Preco,
            currency_id = "BRL",
            available_quantity = item.Quantidade,
            buying_mode = item.BuyingMode,
            description = new { plain_text = item.Descricao },
            pictures = item.UrlsFotos.Select(url => new { source = url }).ToList(),
            listing_type_id = "gold_special"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/items") { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        await GarantirSucessoAsync(response, $"publicar item \"{item.Titulo}\"");

        var resultado = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var itemId = resultado?["id"]?.ToString() ?? throw new InvalidOperationException("Resposta sem id ao publicar item.");

        _logger.LogInformation("Item publicado no ML: {Titulo} -> {ItemId}", item.Titulo, itemId);
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
        _logger.LogError(
            "Falha ao {Operacao} — HTTP {Status}: {Corpo}",
            operacao, (int)response.StatusCode, corpo);

        throw new InvalidOperationException(
            $"Mercado Livre recusou a operação \"{operacao}\" (HTTP {(int)response.StatusCode}): {corpo}");
    }
}
