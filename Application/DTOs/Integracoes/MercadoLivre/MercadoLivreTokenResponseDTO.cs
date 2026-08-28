using System.Text.Json.Serialization;

namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class MercadoLivreTokenResponseDTO
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
}
