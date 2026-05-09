//classe de base para os itens da ordem de servico, usado na ordem de servico

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de ItemOrdemServico.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class ItemOrdemServico : Entity
{
    public Guid ComponenteId { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public int Quantidade { get; private set; }

    public Dinheiro ValorUnitario { get; private set; } = null!;
    public Dinheiro ValorTotal { get; private set; } = null!;

    protected ItemOrdemServico() { }

    public ItemOrdemServico(
        Guid componenteId,
        Guid ordemServicoId,
        int quantidade,
        decimal valorUnitario)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        ComponenteId = componenteId;
        OrdemServicoId = ordemServicoId;
        Quantidade = quantidade;
        ValorUnitario = new Dinheiro(valorUnitario);

        CalcularTotal();
    }

    /* ================================
        metodos GETTERS dos atributos
     ================================*/
    public Guid GetComponentId() => ComponenteId;
    public Guid GetOrdemServicoId() => OrdemServicoId;
    public int GetQuantidade() => Quantidade;
    public decimal GetValorUnitario() => ValorUnitario.GetValorDinheiro();
    public decimal GetValorTotal() => ValorTotal.GetValorDinheiro();

    /* =====================================
        metodos SETTERS de cada atributo
        com regras de negocio aplicadas
     =====================================*/
     
     /*
        altera a quantidade de um item da Ordem de Servico
        deve ser maior ou igual a 0
     */
    public void AlterarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade inválida");

        Quantidade = novaQuantidade;
        CalcularTotal();
    }

    //atualiza o valor unitaio do item
    public void AtualizarValorUnitario(decimal novoValor)
    {
        ValorUnitario = new Dinheiro(novoValor);
        CalcularTotal();
    }

    /*
        calcula o total, multiplica o item pela quantidade 
        que foi informada no formulario da Ordem de Servico
    */
    private void CalcularTotal()
    {
        ValorTotal = ValorUnitario.Multiplicar(Quantidade);
    }
}