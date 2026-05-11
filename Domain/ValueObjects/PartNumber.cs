using System.Text.RegularExpressions;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de Part Number — código do fabricante de uma peça.
    Aceita alfanuméricos com - / . _ (separadores comuns em códigos OEM).
    Normaliza para maiúsculas e remove espaços.
*/
public sealed class PartNumber : IEquatable<PartNumber>
{
    private static readonly Regex _formato = new(@"^[A-Z0-9\-\/\._]{1,50}$", RegexOptions.Compiled);

    public string Valor { get; }

    protected PartNumber() { Valor = null!; }

    public PartNumber(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new PartNumberInvalidoException(valor ?? string.Empty);

        var normalizado = valor.Trim().ToUpperInvariant();

        if (!_formato.IsMatch(normalizado))
            throw new PartNumberInvalidoException(valor);

        Valor = normalizado;
    }

    public override string ToString() => Valor;
    public bool Equals(PartNumber? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => Equals(obj as PartNumber);
    public override int GetHashCode() => Valor.GetHashCode();

    public static implicit operator string(PartNumber pn) => pn.Valor;
}
