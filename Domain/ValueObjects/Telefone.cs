using System.Text.RegularExpressions;

namespace CarStoreManager.Domain.ValueObjects;

public class Telefone
{
    public string Numero { get; } = null!;

    protected Telefone() { } // EF

    public Telefone(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Telefone não pode ser vazio");

        var apenasNumeros = RemoverMascara(numero);

        if (!EhValido(apenasNumeros))
            throw new ArgumentException("Telefone inválido");

        Numero = apenasNumeros;
    }

    // =========================
    // VALIDAÇÃO
    // =========================

    private static string RemoverMascara(string input)
    {
        return Regex.Replace(input, @"\D", "");
    }

    private static bool EhValido(string numero)
    {
        // Brasil: DDD (2) + número (8 ou 9)
        return numero.Length is 10 or 11;
    }

    // =========================
    // FORMATAÇÃO
    // =========================

    public string Formatado()
    {
        if (Numero.Length == 11)
        {
            return $"({Numero[..2]}) {Numero[2..7]}-{Numero[7..]}";
        }

        return $"({Numero[..2]}) {Numero[2..6]}-{Numero[6..]}";
    }

    public override string ToString() => Formatado();

    // =========================
    // Equals
    // =========================

    public override bool Equals(object? obj)
    {
        if (obj is not Telefone other)
            return false;

        return Numero == other.Numero;
    }

    public override int GetHashCode()
        => Numero.GetHashCode();

    public static bool operator ==(Telefone a, Telefone b)
        => a?.Numero == b?.Numero;

    public static bool operator !=(Telefone a, Telefone b)
        => !(a == b);
}