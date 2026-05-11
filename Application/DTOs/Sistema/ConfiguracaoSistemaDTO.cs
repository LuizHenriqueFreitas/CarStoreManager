namespace CarStoreManager.Application.DTOs.Sistema;

public class ConfiguracaoSistemaDTO
{
    public string NomeFinanciadora { get; set; } = "";
    public string EmailFinanciadora { get; set; } = "";

    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsuario { get; set; } = "";
    /// <summary>Não exposta nas respostas GET — sempre vazia ao ler.</summary>
    public string SmtpSenha { get; set; } = "";
    public bool SmtpUsarSsl { get; set; } = true;

    public string EmailRemetente { get; set; } = "";
    public string NomeRemetente { get; set; } = "";

    public DateTime? DataUltimaAtualizacao { get; set; }
    public bool SmtpConfigurado { get; set; }
    public bool FinanciadoraConfigurada { get; set; }
}

public class TestarEmailDTO
{
    public string EmailDestino { get; set; } = "";
}
