using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

namespace CarStoreManager.Application.Interfaces;

/// <summary>
/// Cliente HTTP para a API do Mercado Livre. Implementado por
/// MercadoLivreApiClientReal (HttpClient real, registrado na DI).
/// </summary>
public interface IMercadoLivreApiClient
{
    // === OAuth ===
    Task<MercadoLivreTokenResponseDTO> TrocarCodigoPorTokenAsync(string code);
    Task<MercadoLivreTokenResponseDTO> RenovarTokenAsync(string refreshToken);
    Task<MercadoLivreUsuarioDTO> ObterUsuarioAsync(string accessToken);

    // === Publicação ===
    Task<string> PublicarItemAsync(MercadoLivreItemDTO item, string accessToken);
    Task AtualizarItemAsync(string itemIdML, MercadoLivreItemDTO item, string accessToken);
    Task PausarItemAsync(string itemIdML, string accessToken);
    Task EncerrarItemAsync(string itemIdML, string accessToken);

    // === Vendas ===
    Task<MercadoLivrePedidoDTO> ObterPedidoAsync(string resourceUrl, string accessToken);
}
