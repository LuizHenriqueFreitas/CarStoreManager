namespace CarStoreManager.Domain.Enums;

/// <summary>
/// Formas de pagamento aceitas tanto em PropostaVenda quanto em OrdemServico.
/// Cartão de crédito faz sentido em OS (valores menores) mas raramente para
/// veículo completo (limite tipicamente insuficiente). Mantido como opção
/// porque algumas concessionárias aceitam para entrada.
/// </summary>
public enum ModoPagamento
{
    NaoDefinido = 0,
    Dinheiro = 1,
    Pix = 2,
    CartaoDebito = 3,
    CartaoCredito = 4,
    Financiamento = 5,
    Boleto = 6,
    Transferencia = 7
}
