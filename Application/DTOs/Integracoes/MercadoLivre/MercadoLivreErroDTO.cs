using System.Text.Json.Serialization;

namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

/// <summary>
/// Formato de erro da API do ML: {"message","error","status","cause":[...]}. Os
/// itens de "cause" não têm um formato 100% fixo entre endpoints — por isso os
/// campos aqui são todos opcionais e a desserialização em
/// <c>MercadoLivreApiClientReal.TraduzirErro</c> é sempre best-effort, nunca a
/// única fonte da mensagem (o corpo cru é sempre preservado à parte).
/// </summary>
public class MercadoLivreErroDTO
{
    public string? Message { get; set; }
    public string? Error { get; set; }
    public int? Status { get; set; }
    public List<MercadoLivreErroCausaDTO> Cause { get; set; } = new();
}

public class MercadoLivreErroCausaDTO
{
    public string? Code { get; set; }
    public string? Message { get; set; }

    [JsonPropertyName("references")]
    public List<string>? Referencias { get; set; }
}
