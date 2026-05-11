using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de CEST (Código Especificador da Substituição Tributária).
    Opcional: aceita string vazia. Quando preenchido, sempre 7 dígitos.
    Formato canônico de exibição: XX.XXX.XX
*/
public sealed class Cest : IEquatable<Cest>
{
    public string Valor { get; }

    public bool EstaPreenchido => !string.IsNullOrEmpty(Valor);

    public static Cest Vazio { get; } = new();

    protected Cest() { Valor = string.Empty; }

    public Cest(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Valor = string.Empty;
            return;
        }

        var limpo = valor.Replace(".", "").Trim();

        if (limpo.Length != 7 || !limpo.All(char.IsDigit))
            throw new CestInvalidoException(valor);

        Valor = limpo;
    }

    public string Formatado() => EstaPreenchido
        ? $"{Valor.Substring(0, 2)}.{Valor.Substring(2, 3)}.{Valor.Substring(5, 2)}"
        : string.Empty;

    public override string ToString() => Valor;
    public bool Equals(Cest? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => Equals(obj as Cest);
    public override int GetHashCode() => Valor.GetHashCode();

    public static implicit operator string(Cest cest) => cest.Valor;
}
