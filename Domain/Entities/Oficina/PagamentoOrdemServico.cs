using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Registro de cobrança/recebimento de uma OS. Uma OS pode ter múltiplos
/// pagamentos (ex.: entrada via cartão débito + restante via PIX).
/// O status de pagamento da OS é derivado da soma dos valores aqui registrados
/// vs. o ValorTotal da OS (Pendente / Parcial / Pago).
/// </summary>
public class PagamentoOrdemServico : Entity
{
    public Guid OrdemServicoId { get; private set; }

    public ModoPagamento ModoPagamento { get; private set; }
    public Dinheiro Valor { get; private set; } = null!;
    public DateTime DataPagamento { get; private set; }

    /// <summary>Usuário (recepcionista normalmente) que registrou o recebimento.</summary>
    public Guid RecebidoPor { get; private set; }

    /// <summary>
    /// Identificador externo (ex.: NSU do cartão, end-to-end do PIX, número
    /// de protocolo bancário). Opcional mas útil para conciliação.
    /// </summary>
    public string? ReferenciaExterna { get; private set; }

    public string? Observacoes { get; private set; }

    protected PagamentoOrdemServico() { }

    public PagamentoOrdemServico(
        Guid ordemServicoId,
        ModoPagamento modoPagamento,
        decimal valor,
        Guid recebidoPor,
        string? referenciaExterna = null,
        string? observacoes = null)
    {
        if (modoPagamento == ModoPagamento.NaoDefinido)
            throw new ArgumentException("Modo de pagamento é obrigatório.", nameof(modoPagamento));
        if (valor <= 0)
            throw new ArgumentException("Valor do pagamento deve ser positivo.", nameof(valor));
        if (recebidoPor == Guid.Empty)
            throw new ArgumentException("Usuário recebedor é obrigatório.", nameof(recebidoPor));

        OrdemServicoId = ordemServicoId;
        ModoPagamento = modoPagamento;
        Valor = new Dinheiro(valor);
        DataPagamento = DateTime.UtcNow;
        RecebidoPor = recebidoPor;
        ReferenciaExterna = referenciaExterna?.Trim();
        Observacoes = observacoes?.Trim();
    }
}
