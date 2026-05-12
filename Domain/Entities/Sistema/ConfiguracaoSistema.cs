using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Singleton — existe apenas UM registro no BD com todas as configurações
/// gerenciáveis pelo administrador via UI. Inclui credenciais SMTP e dados
/// da financiadora preferida do sistema.
/// </summary>
public class ConfiguracaoSistema : Entity
{
    // === Financiadora preferida ===
    public string NomeFinanciadora { get; private set; } = "";
    public string EmailFinanciadora { get; private set; } = "";

    // === Configurações de envio de e-mail (SMTP) ===
    public string SmtpHost { get; private set; } = "";
    public int SmtpPort { get; private set; } = 587;
    public string SmtpUsuario { get; private set; } = "";
    public string SmtpSenha { get; private set; } = "";
    public bool SmtpUsarSsl { get; private set; } = true;

    /// <summary>Endereço "From" usado nos e-mails enviados pelo sistema.</summary>
    public string EmailRemetente { get; private set; } = "";
    /// <summary>Nome amigável "From" (ex: "CarStore Concessionária").</summary>
    public string NomeRemetente { get; private set; } = "";

    // === Margens de lucro por sistema do componente ===
    /// <summary>
    /// Margens persistidas como JSON (chave = nome do enum SistemaComponente,
    /// valor = percentual). Usadas como padrão ao cadastrar componentes.
    /// </summary>
    public string MargensPorSistemaJson { get; private set; } = "{}";

    /// <summary>Margem fallback quando o sistema do componente não está mapeado.</summary>
    public decimal MargemPadraoGlobalPct { get; private set; } = 30m;

    public DateTime? DataUltimaAtualizacao { get; private set; }

    protected ConfiguracaoSistema() { }

    /// <summary>
    /// Construtor para a primeira inicialização — todos os campos vazios.
    /// O admin preenche tudo via UI antes do primeiro uso real.
    /// </summary>
    public ConfiguracaoSistema(bool _) : this() { }

    public void AtualizarFinanciadora(string nome, string email)
    {
        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
            throw new ArgumentException("E-mail da financiadora inválido.", nameof(email));

        NomeFinanciadora = (nome ?? "").Trim();
        EmailFinanciadora = (email ?? "").Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarSmtp(
        string host,
        int porta,
        string usuario,
        string senha,
        bool usarSsl,
        string emailRemetente,
        string nomeRemetente)
    {
        if (porta <= 0 || porta > 65535)
            throw new ArgumentException("Porta SMTP inválida.", nameof(porta));
        if (!string.IsNullOrWhiteSpace(emailRemetente) && !emailRemetente.Contains('@'))
            throw new ArgumentException("E-mail remetente inválido.", nameof(emailRemetente));

        SmtpHost = (host ?? "").Trim();
        SmtpPort = porta;
        SmtpUsuario = (usuario ?? "").Trim();
        SmtpSenha = senha ?? "";
        SmtpUsarSsl = usarSsl;
        EmailRemetente = (emailRemetente ?? "").Trim();
        NomeRemetente = (nomeRemetente ?? "").Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se o SMTP foi configurado o suficiente para tentar enviar.
    /// </summary>
    public bool SmtpConfigurado()
        => !string.IsNullOrWhiteSpace(SmtpHost)
           && !string.IsNullOrWhiteSpace(EmailRemetente);

    public bool FinanciadoraConfigurada()
        => !string.IsNullOrWhiteSpace(EmailFinanciadora);

    /// <summary>
    /// Atualiza margens — recebe dictionary nome do sistema → percentual.
    /// Persistido como JSON simples.
    /// </summary>
    public void AtualizarMargens(IDictionary<string, decimal> margensPorSistema, decimal padraoGlobal)
    {
        if (padraoGlobal < 0)
            throw new ArgumentException("Margem padrão não pode ser negativa.", nameof(padraoGlobal));

        var sane = margensPorSistema?
            .Where(kv => kv.Value >= 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value)
            ?? new Dictionary<string, decimal>();

        MargensPorSistemaJson = System.Text.Json.JsonSerializer.Serialize(sane);
        MargemPadraoGlobalPct = padraoGlobal;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Devolve a margem aplicável a um sistema específico — retorna padrão
    /// global se o sistema não está mapeado.
    /// </summary>
    public decimal ObterMargemParaSistema(Domain.Enums.SistemaComponente? sistema)
    {
        if (sistema is null) return MargemPadraoGlobalPct;
        try
        {
            var dict = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, decimal>>(MargensPorSistemaJson)
                ?? new();
            return dict.TryGetValue(sistema.Value.ToString(), out var pct)
                ? pct : MargemPadraoGlobalPct;
        }
        catch
        {
            return MargemPadraoGlobalPct;
        }
    }
}
