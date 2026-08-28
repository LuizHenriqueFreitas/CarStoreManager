namespace CarStoreManager.Application.Common;

/// <summary>Config estática do app Mercado Livre — bindada de appsettings.json ("MercadoLivre").</summary>
public class MercadoLivreConfig
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RedirectUri { get; set; } = "";

    /// <summary>"Sandbox" ou "Production" — hoje apenas metadado/exibição, não altera a URL base da API (ver observação no MercadoLivreApiClientReal).</summary>
    public string Ambiente { get; set; } = "Sandbox";

    /// <summary>Host público (https) usado para transformar URLs relativas de Foto em URLs absolutas exigidas pela API do ML.</summary>
    public string UrlBasePublica { get; set; } = "";
}
