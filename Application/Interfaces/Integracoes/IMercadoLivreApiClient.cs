using System.Text.Json;
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
    /// <param name="payload">Objeto já pronto pra serializar — montado por IConstrutorPayloadAnuncio, nunca fixo aqui.</param>
    Task<string> PublicarItemAsync(object payload, string accessToken);
    Task AtualizarItemAsync(string itemIdML, MercadoLivreItemDTO item, string accessToken);
    Task PausarItemAsync(string itemIdML, string accessToken);
    Task EncerrarItemAsync(string itemIdML, string accessToken);

    // === Catálogo (descoberta de categoria, atributos, modalidades — tudo em runtime, nada fixo) ===

    /// <summary>GET /sites/{site}/domain_discovery/search?q= — sugere a categoria mais provável a partir de um título. Não exige token.</summary>
    Task<JsonElement> SugerirCategoriaAsync(string texto);

    /// <summary>GET /categories/{id} — inclui o bloco "settings" (buying_modes, se é classificado, etc). Não exige token.</summary>
    Task<JsonElement> ObterCategoriaAsync(string categoriaId);

    /// <summary>GET /categories/{id}/attributes — atributos com tags.required e, pra atributos de lista, os "values" com id/name. Não exige token.</summary>
    Task<JsonElement> ObterAtributosCategoriaAsync(string categoriaId);

    /// <summary>GET /users/{userId}/available_listing_types?category_id= — modalidades realmente disponíveis pra ESSA conta nessa categoria (é o que resolve o listing_type.invalid). Exige token.</summary>
    Task<JsonElement> ObterModalidadesAnuncioAsync(string categoriaId, string userId, string accessToken);

    // === Vendas ===
    Task<MercadoLivrePedidoDTO> ObterPedidoAsync(string resourceUrl, string accessToken);
}
