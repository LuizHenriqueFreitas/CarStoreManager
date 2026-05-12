using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Concessionaria;

/// <summary>
/// Registro de cobrança/recebimento referente a uma PropostaVenda.
/// Uma proposta pode acumular múltiplos pagamentos parciais (ex.: entrada via
/// PIX, restante via financiamento/transferência). O status de pagamento da
/// proposta é derivado dinamicamente da soma desses pagamentos vs ValorFinal.
/// </summary>
public class PagamentoProposta : Entity
{
    public Guid PropostaVendaId { get; private set; }

    public ModoPagamento ModoPagamento { get; private set; }
    public Dinheiro Valor { get; private set; } = null!;
    public DateTime DataPagamento { get; private set; }

    /// <summary>Admin/vendedor que registrou o recebimento.</summary>
    public Guid RecebidoPor { get; private set; }

    /// <summary>NSU do cartão, end-to-end PIX, número de protocolo bancário.</summary>
    public string? ReferenciaExterna { get; private set; }

    public string? Observacoes { get; private set; }

    protected PagamentoProposta() { }

    public PagamentoProposta(
        Guid propostaVendaId,
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

        PropostaVendaId = propostaVendaId;
        ModoPagamento = modoPagamento;
        Valor = new Dinheiro(valor);
        DataPagamento = DateTime.UtcNow;
        RecebidoPor = recebidoPor;
        ReferenciaExterna = referenciaExterna?.Trim();
        Observacoes = observacoes?.Trim();
    }
}
