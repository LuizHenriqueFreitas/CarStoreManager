namespace CarStoreManager.Domain.Enums;

/*
    De onde vem a peça que será usada na Ordem de Serviço.
    - Estoque   : oficina já tem; sai do EstoqueComponente.
    - Cliente   : cliente trouxe a peça e pediu apenas a instalação.
    - Encomenda : oficina precisa comprar/aguardar chegar.
*/
public enum OrigemItemOrdemServico
{
    Estoque = 1,
    Cliente = 2,
    Encomenda = 3
}
