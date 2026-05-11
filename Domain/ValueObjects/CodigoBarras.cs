using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de Código de Barras GTIN/EAN.
    Aceita 8, 12, 13 ou 14 dígitos (EAN-8, UPC-A, EAN-13, GTIN-14).
    Opcional: aceita string vazia.
*/
public sealed class CodigoBarras : IEquatable<CodigoBarras>
{
    private static readonly int[] _comprimentosValidos = { 8, 12, 13, 14 };

    public string Valor { get; }

    public bool EstaPreenchido => !string.IsNullOrEmpty(Valor);

    public static CodigoBarras Vazio { get; } = new();

    protected CodigoBarras() { Valor = string.Empty; }

    public CodigoBarras(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Valor = string.Empty;
            return;
        }

        var limpo = valor.Trim();

        if (!_comprimentosValidos.Contains(limpo.Length) || !limpo.All(char.IsDigit))
            throw new CodigoBarrasInvalidoException(valor);

        Valor = limpo;
    }

    public override string ToString() => Valor;
    public bool Equals(CodigoBarras? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => Equals(obj as CodigoBarras);
    public override int GetHashCode() => Valor.GetHashCode();

    public static implicit operator string(CodigoBarras cb) => cb.Valor;
}
