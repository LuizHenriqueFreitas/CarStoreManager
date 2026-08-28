using System.Text.Json.Serialization;

namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

/// <summary>Resposta de GET /users/me — usada após o OAuth para saber qual conta foi conectada.</summary>
public class MercadoLivreUsuarioDTO
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
}
