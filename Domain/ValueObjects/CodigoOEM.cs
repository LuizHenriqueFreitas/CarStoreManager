using System.Text.RegularExpressions;
using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de Código OEM — código original do fabricante do veículo.
    Opcional: aceita string vazia. Quando preenchido, segue mesmas regras
    do PartNumber (alfanuméricos com - / . _, até 50 chars).
*/
public sealed class CodigoOEM : IEquatable<CodigoOEM>
{
    private static readonly Regex _formato = new(@"^[A-Z0-9\-\/\._]{1,50}$", RegexOptions.Compiled);

    public string Valor { get; }

    public bool EstaPreenchido => !string.IsNullOrEmpty(Valor);

    public static CodigoOEM Vazio { get; } = new();

    protected CodigoOEM() { Valor = string.Empty; }

    public CodigoOEM(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            Valor = string.Empty;
            return;
        }

        var normalizado = valor.Trim().ToUpperInvariant();

        if (!_formato.IsMatch(normalizado))
            throw new CodigoOEMInvalidoException(valor);

        Valor = normalizado;
    }

    public override string ToString() => Valor;
    public bool Equals(CodigoOEM? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => Equals(obj as CodigoOEM);
    public override int GetHashCode() => Valor.GetHashCode();

    public static implicit operator string(CodigoOEM oem) => oem.Valor;
}
