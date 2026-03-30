using System.Text.RegularExpressions;

namespace CarStoreManager.Domain.ValueObjects;

public class Email : IEquatable<Email>
{
    public string Endereco { get; }

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("Email não pode ser vazio");

        endereco = endereco.Trim().ToLower();

        if (!EmailRegex.IsMatch(endereco))
            throw new ArgumentException("Email inválido");

        Endereco = endereco;
    }

    // =========================
    // EQUALS (IMPORTANTE PRA VO)
    // =========================

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Endereco == other.Endereco;
    }

    public override bool Equals(object? obj)
        => obj is Email other && Equals(other);

    public override int GetHashCode()
        => Endereco.GetHashCode();

    public static bool operator ==(Email a, Email b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Email a, Email b)
        => !Equals(a, b);

    // =========================
    // UTILIDADE
    // =========================

    public override string ToString()
        => Endereco;
}