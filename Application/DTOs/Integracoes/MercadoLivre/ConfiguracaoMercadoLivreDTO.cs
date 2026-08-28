namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class ConfiguracaoMercadoLivreDTO
{
    public bool Conectado { get; set; }
    public string? EmailContaConectada { get; set; }
    public DateTime? DataConexao { get; set; }
    public DateTime? DataExpiracaoToken { get; set; }

    /// <summary>Espelha MercadoLivreConfig.Ambiente — somente leitura, definido em appsettings.json (requer reinício para alterar).</summary>
    public string Ambiente { get; set; } = "";
}
