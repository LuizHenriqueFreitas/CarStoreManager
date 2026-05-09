//acrescentar sistema de juros futuramente
namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Financiamento, como todos 
    os metodos get e set de cada atributo, alem do Calculo
    automatico do valor das parcelas, e tambem 
    metodo de validação do financiamento

    Testes automaticos implementados para:
        Criar Financiamento Valido,
        Atualizar um financiamento valido,
        Verificar o calculo de valor financiado,
        Verificar o calculo de valor da parcela,
        Verificar que o valor da parcela é arredondado,
        Bloquear entrada menor que 500,
        Bloquear entrada maior que o total,
        Bloquear entrada igual ao total
*/

public class Financiamento
{
    public Dinheiro ValorTotal { get; private set; } = null!;
    public Dinheiro ValorEntrada { get; private set; } = null!;
    public Parcelas Parcelas { get; private set; } = null!;
    public Dinheiro ValorFinanciado { get; private set; } = null!;
    public Dinheiro ValorParcela { get; private set; } = null!;

    protected Financiamento () {}

    //metodo construtor do financiamento
    public Financiamento(decimal valorTotal, int parcelas, decimal entrada)
    {
        //verifique que nem valor total, quantidade de parcelas ou entrada seja vazio
        if (valorTotal < 0 || parcelas < 0 || entrada < 0)
            throw new ArgumentNullException("Erro nas informações do financiamento.");

        //verifica que o valor da entrada nao seja maior que o valor total
        if (entrada >= valorTotal)
            throw new ArgumentException("Entrada não pode ser igual ou maior que o valor total");

        //verifica que a entrada não seja menor que 500
        if (entrada < 500)
            throw new ArgumentException("Entrada não pode ser menor que 500");


        ValorTotal = new Dinheiro(valorTotal);
        Parcelas = new Parcelas(parcelas);
        ValorEntrada = new Dinheiro(entrada);

        ValorFinanciado = ValorTotal.Subtrair(ValorEntrada);
        ValorParcela = CalcularValorParcela();
    }

    /*
        Abaixo nós temos:
        Metodos getters e setters para todos os atributos
    */

    public decimal GetValorTotal()
    {
        return ValorTotal.GetValorDinheiro();
    }

    public decimal GetValorEntrada()
    {
        return ValorEntrada.GetValorDinheiro();
    }

    public int GetQuantidadeParcelas()
    {
        return Parcelas.GetParcelasQuantidade();
    }

    public decimal GetValorFinanciado()
    {
        return ValorFinanciado.GetValorDinheiro();
    }

    public decimal GetValorParcela()
    {
        return ValorParcela.GetValorDinheiro();
    }

    public void SetValorTotal(Dinheiro valor)
    {
        ValorTotal = valor;
    }
    public void SetValorEntrada(Dinheiro valor)
    {
        ValorEntrada = valor;
    }
    public void SetQuantidadeParcela(Parcelas valor)
    {
        Parcelas = valor;
    }

    //Metodo de atualização de um funanciamento com validação
    public void AtualizarFinanciamento(decimal novoTotal, int quantParcelas, decimal entrada)
    {
        Financiamento novoFinan = new Financiamento(novoTotal, quantParcelas, entrada);

        if(!ValidaFinanciamento(novoFinan))
            throw new ArgumentException("Erro nas informações do novo financimento");
        
        //atualizando os campos manuais
        SetValorTotal(new Dinheiro(novoFinan.GetValorTotal()));
        SetValorEntrada(new Dinheiro(novoFinan.GetValorEntrada()));
        SetQuantidadeParcela(new Parcelas(novoFinan.GetQuantidadeParcelas()));
    }

    //metodo que valida se o financiamento segue as regras de negocios
    public bool ValidaFinanciamento(Financiamento finan)
    {
        //verifique que nem valor total, quantidade de parcelas ou entrada seja vazio
        if (new Dinheiro(finan.GetValorTotal()) is null || 
                new Parcelas(finan.GetQuantidadeParcelas()) is null || 
                new Dinheiro(finan.GetValorEntrada()) is null)
            return false;

        //verifica que o valor da entrada nao seja maior que o valor total
        if (finan.GetValorEntrada() >= finan.GetValorTotal())
            return false;

        //verifica que a entrada não seja menor que 500
        if (finan.GetValorEntrada() < 500)
            return false;

        
        return true;
    }

    //calcula as parcelas automaticamente
    private Dinheiro CalcularValorParcela()
    {
        if (Parcelas.GetParcelasQuantidade() == 0)
            return new Dinheiro(0);

        var valor = GetValorFinanciado() / GetQuantidadeParcelas();
        return new Dinheiro(valor);
    }
}