namespace CarStoreManager.Domain.Enums;

/*
    Status físico de cada peça atrelada à OS:
    - Disponivel        : pronta para uso (estoque ou trazida pelo cliente).
    - AguardandoChegada : encomendada, aguardando chegar à oficina.
    - Recebido          : encomenda chegou (entrada no estoque) e está pronta para uso.
*/
public enum StatusItemOrdemServico
{
    Disponivel = 1,
    AguardandoChegada = 2,
    Recebido = 3
}
