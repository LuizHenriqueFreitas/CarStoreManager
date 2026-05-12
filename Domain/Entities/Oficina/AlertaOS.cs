using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Alerta emitido pelo mecânico durante OS EmAndamento — sinaliza que ele
/// achou avaria/problema novo que aumenta o escopo. A OS é pausada até o
/// cliente decidir (via recepção): aprovar (escopo aumentado) ou recusar
/// (continua só com o trabalho originalmente combinado).
/// </summary>
public class AlertaOS : Entity
{
    public Guid OrdemServicoId { get; private set; }
    public Guid MecanicoId { get; private set; }

    public string Descricao { get; private set; } = "";

    public StatusAlertaOS Status { get; private set; }

    public DateTime DataCriacao { get; private set; }
    public DateTime? DataResolucao { get; private set; }
    public Guid? ResolvidoPor { get; private set; }
    public string? ObservacaoCliente { get; private set; }

    protected AlertaOS() { }

    public AlertaOS(Guid ordemServicoId, Guid mecanicoId, string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do alerta é obrigatória.", nameof(descricao));

        OrdemServicoId = ordemServicoId;
        MecanicoId = mecanicoId;
        Descricao = descricao.Trim();
        Status = StatusAlertaOS.Pendente;
        DataCriacao = DateTime.UtcNow;
    }

    public void RegistrarDecisaoCliente(bool aprovou, Guid resolvidoPor, string? observacaoCliente = null)
    {
        if (Status != StatusAlertaOS.Pendente)
            throw new InvalidOperationException($"Alerta já resolvido (status {Status}).");

        Status = aprovou ? StatusAlertaOS.ClienteAprovou : StatusAlertaOS.ClienteRecusou;
        ResolvidoPor = resolvidoPor;
        ObservacaoCliente = observacaoCliente?.Trim();
        DataResolucao = DateTime.UtcNow;
    }
}
