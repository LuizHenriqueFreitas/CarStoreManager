namespace CarStoreManager.Domain.ValueObjects;

public class Dinheiro : IEquatable<Dinheiro>
{
    public decimal Valor { get; }

    public Dinheiro(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor não pode ser negativo");

        Valor = decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    // =========================
    // OPERAÇÕES BÁSICAS
    // =========================

    public Dinheiro Somar(Dinheiro outro)
    {
        ValidarOutro(outro);
        return new Dinheiro(Valor + outro.Valor);
    }

    public Dinheiro Subtrair(Dinheiro outro)
    {
        ValidarOutro(outro);

        var resultado = Valor - outro.Valor;

        if (resultado < 0)
            throw new InvalidOperationException("Resultado não pode ser negativo");

        return new Dinheiro(resultado);
    }

    public Dinheiro Multiplicar(int quantidade)
    {
        if (quantidade < 0)
            throw new ArgumentException("Quantidade inválida");

        return new Dinheiro(Valor * quantidade);
    }

    public Dinheiro Dividir(int divisor)
    {
        if (divisor <= 0)
            throw new ArgumentException("Divisor inválido");

        return new Dinheiro(Valor / divisor);
    }

    // =========================
    // COMPARAÇÕES
    // =========================

    public bool MaiorQue(Dinheiro outro)
    {
        ValidarOutro(outro);
        return Valor > outro.Valor;
    }

    public bool MenorQue(Dinheiro outro)
    {
        ValidarOutro(outro);
        return Valor < outro.Valor;
    }

    public bool Igual(Dinheiro outro)
    {
        ValidarOutro(outro);
        return Valor == outro.Valor;
    }

    // =========================
    // OPERADORES 
    // =========================

    public static Dinheiro operator +(Dinheiro a, Dinheiro b)
        => a.Somar(b);

    public static Dinheiro operator -(Dinheiro a, Dinheiro b)
        => a.Subtrair(b);

    public static Dinheiro operator *(Dinheiro a, int b)
        => a.Multiplicar(b);

    public static Dinheiro operator /(Dinheiro a, int b)
        => a.Dividir(b);

    public static bool operator >(Dinheiro a, Dinheiro b)
        => a.MaiorQue(b);

    public static bool operator <(Dinheiro a, Dinheiro b)
        => a.MenorQue(b);

    public static bool operator ==(Dinheiro a, Dinheiro b)
        => Equals(a, b);

    public static bool operator !=(Dinheiro a, Dinheiro b)
        => !Equals(a, b);

    // =========================
    // EQUALS / HASHCODE
    // =========================

    public bool Equals(Dinheiro? other)
    {
        if (other is null) return false;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj)
        => obj is Dinheiro other && Equals(other);

    public override int GetHashCode()
        => Valor.GetHashCode();

    // =========================
    // UTILIDADES
    // =========================

    public override string ToString()
        => $"R$ {Valor:N2}";

    private static void ValidarOutro(Dinheiro outro)
    {
        if (outro is null)
            throw new ArgumentNullException(nameof(outro));
    }
}