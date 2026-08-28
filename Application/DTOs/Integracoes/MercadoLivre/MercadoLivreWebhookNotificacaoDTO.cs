using System.Text.Json.Serialization;

namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

/// <summary>
/// Formato real das notificações do Mercado Livre: um ponteiro de recurso
/// (topic + resource), não o pedido completo — o processamento sempre busca
/// os dados reais via GET no "resource" antes de confiar em qualquer coisa.
/// </summary>
public class MercadoLivreWebhookNotificacaoDTO
{
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = "";

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = "";

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("application_id")]
    public long ApplicationId { get; set; }
}
