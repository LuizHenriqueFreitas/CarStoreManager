using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Sistema;

/// <summary>
/// Singleton — existe apenas UM registro no BD com todas as configurações
/// gerenciáveis pelo administrador via UI.
/// </summary>
public class ConfiguracaoSistema : Entity
{
    // === Margens de lucro por sistema do componente ===
    /// <summary>
    /// Margens persistidas como JSON (chave = nome do enum SistemaComponente,
    /// valor = percentual). Usadas como padrão ao cadastrar componentes.
    /// </summary>
    public string MargensPorSistemaJson { get; private set; } = "{}";

    /// <summary>Margem fallback quando o sistema do componente não está mapeado.</summary>
    public decimal MargemPadraoGlobalPct { get; private set; } = 30m;

    // === Modo operante da oficina — entrada mínima opcional ===
    /// <summary>
    /// Quando true, a OS só pode sair de <c>Pendente/Aprovada</c> para <c>EmAndamento</c>
    /// após o cliente pagar pelo menos <c>PercentualEntradaMinima%</c> do valor total.
    /// </summary>
    public bool ExigirEntradaMinima { get; private set; } = false;

    /// <summary>Percentual mínimo de entrada exigido (0–100). Ignorado se <see cref="ExigirEntradaMinima"/> for false.</summary>
    public decimal PercentualEntradaMinima { get; private set; } = 0m;

    // === Templates de documentos (texto livre, com lacunas para o usuário preencher) ===
    /// <summary>Texto-base do termo de entrega, usado para pré-preencher o cadastro de novo veículo.</summary>
    public string TemplateTermoEntrega { get; private set; } = TemplatesDocumentosPadrao.TermoEntrega;

    /// <summary>Texto-base do contrato de consignação, usado para pré-preencher o cadastro de veículo consignado.</summary>
    public string TemplateContratoConsignacao { get; private set; } = TemplatesDocumentosPadrao.ContratoConsignacao;

    public DateTime? DataUltimaAtualizacao { get; private set; }

    protected ConfiguracaoSistema() { }

    /// <summary>
    /// Construtor para a primeira inicialização — todos os campos vazios.
    /// O admin preenche tudo via UI antes do primeiro uso real.
    /// </summary>
    public ConfiguracaoSistema(bool _) : this() { }

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
    /// Configura o modo operante de entrada mínima para abertura de OS.
    /// Quando <paramref name="exigir"/> é false, o percentual é zerado.
    /// </summary>
    public void ConfigurarEntradaMinima(bool exigir, decimal percentual)
    {
        if (exigir)
        {
            if (percentual < 0 || percentual > 100)
                throw new ArgumentException("Percentual de entrada mínima deve estar entre 0 e 100.", nameof(percentual));
            PercentualEntradaMinima = decimal.Round(percentual, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            PercentualEntradaMinima = 0m;
        }

        ExigirEntradaMinima = exigir;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza os templates de documentos. Aceita texto vazio (loja que
    /// prefere redigir cada documento do zero, sem ponto de partida).
    /// </summary>
    public void AtualizarTemplates(string templateTermoEntrega, string templateContratoConsignacao)
    {
        TemplateTermoEntrega = templateTermoEntrega ?? "";
        TemplateContratoConsignacao = templateContratoConsignacao ?? "";
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
