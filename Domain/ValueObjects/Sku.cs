using System.Text.RegularExpressions;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de SKU interno — código de controle interno do produto.
    Aceita alfanuméricos e separadores comuns. Sempre normalizado para maiúsculas.
*/
public sealed class Sku : IEquatable<Sku>
{
    private static readonly Regex _formato = new(@"^[A-Z0-9\-\/\._]{1,50}$", RegexOptions.Compiled);

    public string Valor { get; }

    protected Sku() { Valor = null!; }

    public Sku(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new SkuInvalidoException(valor ?? string.Empty);

        var normalizado = valor.Trim().ToUpperInvariant();

        if (!_formato.IsMatch(normalizado))
            throw new SkuInvalidoException(valor);

        Valor = normalizado;
    }

    public override string ToString() => Valor;
    public bool Equals(Sku? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => Equals(obj as Sku);
    public override int GetHashCode() => Valor.GetHashCode();

    public static implicit operator string(Sku sku) => sku.Valor;
}
