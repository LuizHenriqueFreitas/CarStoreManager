namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Status de pagamento da OS — derivado dinamicamente da soma dos pagamentos
/// vs ValorTotal. Não é persistido diretamente, calculado pelo service.
/// </summary>
public enum StatusPagamentoOrdemServico
{
    Pendente = 0,
    Parcial = 1,
    Pago = 2
}
