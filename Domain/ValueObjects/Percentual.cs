namespace CarStoreManager.Domain.ValueObjects;

public class Percentual : IEquatable<Percentual>
{
    public decimal Valor { get; }

    private Percentual(decimal valor)
    {
        Valor = valor;
    }

    public static Percentual Criar(decimal valor)
    {
        if (valor < 0 || valor > 100)
            throw new ArgumentException("Percentual deve estar entre 0 e 100");

        return new Percentual(valor);
    }

    public static Percentual Zero() => new Percentual(0);

    // =========================
    // COMPORTAMENTO
    // =========================

    public Dinheiro CalcularValor(Dinheiro baseValor)
    {
        var resultado = baseValor.Valor * (Valor / 100m);
        return new Dinheiro(resultado);
    }

    public decimal ParaDecimal()
        => Valor / 100m;

    // =========================
    // EQUALITY (VO)
    // =========================

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

    public override string ToString()
        => $"{Valor}%";
}