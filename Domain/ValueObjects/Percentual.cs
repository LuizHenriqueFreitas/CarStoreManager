namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Percentuais, alem 
    dos metodos get e set,

    Testes automaticos implementados para:
        Criar percentual valido,
        Bloquear percentuais Invalidos,
        Valida apresentação do valor do desconto,
        Valida apresentação do desconto em percentual,
        Valida Atualização do desconto
*/

public class Percentual : IEquatable<Percentual>
{
    private decimal Valor { get; set; }

    protected Percentual () {}

    //metodo construtos com validação
    public Percentual(decimal valor)
    {
        if (!ValidaDesconto(valor))
            throw new ArgumentException("Percentual deve estar entre 0 e 100");

        Valor = valor;
    }

    //pega o valor do desconto com base no valor total
    public decimal GetDescontoValor()
    {
        return Valor;
    }

    //pega a porcentagem de desconto
    public string GetDescontoPercentual()
    {
        Console.WriteLine($"Valor interno: {Valor}");
        return $"{Valor}%";
    }

    //metodo de atualizar o desconto
    public void AtualizarPercentual(decimal valor)
    {
        if (!ValidaDesconto(valor))
            throw new ArgumentException("Percentual deve estar entre 0 e 100");

        Valor = valor;
    }

    //metodo que valida o desconto de 0 até 100
    public bool ValidaDesconto(decimal valor)
    {
        if (valor < 0 || valor > 100)
            return false;

        return true;
    }

    //colocar o percentual como zero
    public static Percentual Zero() => new Percentual(0);

    //calcula o valor em dinheiro
    public Dinheiro CalcularDescontoValor(Dinheiro baseValor)
    {
        var resultado = baseValor.GetValorDinheiro() * Valor / 100m;
        return new Dinheiro(resultado);
    }

    //funcoes utilitarias de bibliotecas inclusas
    public bool Equals(Percentual? other)
    {
        if (other is null) return false;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj)
        => obj is Percentual other && Equals(other);

    public override int GetHashCode()
        => Valor.GetHashCode();

    public static bool operator ==(Percentual a, Percentual b)
        => Equals(a, b);

    public static bool operator !=(Percentual a, Percentual b)
        => !Equals(a, b);
}