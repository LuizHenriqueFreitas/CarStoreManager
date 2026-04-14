//acrescentar sistema de juros futuramente

namespace CarStoreManager.Domain.ValueObjects;

public class Financiamento
{
    public Dinheiro ValorTotal { get; }
    public Dinheiro Entrada { get; }
    public Parcelas Parcelas { get; }

    public Dinheiro ValorFinanciado => ValorTotal.Subtrair(Entrada);

    public Dinheiro ValorParcela => CalcularValorParcela();

    protected Financiamento () {}

    public Financiamento(Dinheiro valorTotal, Parcelas parcelas, Dinheiro entrada)
    {
        if (valorTotal is null || parcelas is null || entrada is null)
            throw new ArgumentNullException("Erro nas informações do financiamento.");

        if (entrada.Valor > valorTotal.Valor)
            throw new ArgumentException("Entrada não pode ser maior que o valor total");

        ValorTotal = valorTotal;
        Parcelas = parcelas;
        Entrada = entrada;
    }

    private Dinheiro CalcularValorParcela()
    {
        var valor = ValorFinanciado.Valor / Parcelas.Quantidade;
        return new Dinheiro(valor);
    }
}