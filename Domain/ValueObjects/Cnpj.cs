using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de CNPJ — valida dígitos verificadores (algoritmo módulo 11).
    Armazena sem formatação; expõe via ToString() formatado XX.XXX.XXX/XXXX-XX.
*/
public sealed class Cnpj : IEquatable<Cnpj>
{
    private static readonly int[] _multiplicadores1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] _multiplicadores2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

    public string Numero { get; } = null!;

    protected Cnpj() { }

    public Cnpj(string cnpj)
    {
        if (!Validar(cnpj))
            throw new CnpjInvalidoException(cnpj ?? string.Empty);

        Numero = Limpar(cnpj!);
    }

    public string GetNumeroCnpj() => Numero;

    public override string ToString()
        => Convert.ToUInt64(Numero).ToString(@"00\.000\.000\/0000\-00");

    private static string Limpar(string cnpj)
        => cnpj.Replace(".", "")
               .Replace("-", "")
               .Replace("/", "")
               .Trim();

    public static bool Validar(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        cnpj = Limpar(cnpj);

        if (cnpj.Length != 14)
            return false;

        if (cnpj.All(c => c == cnpj[0]))
            return false;

        if (!cnpj.All(char.IsDigit))
            return false;

        var numeros = cnpj.Select(c => int.Parse(c.ToString())).ToArray();

        var soma1 = 0;
        for (var i = 0; i < 12; i++)
            soma1 += numeros[i] * _multiplicadores1[i];

        var resto1 = soma1 % 11;
        var dig1 = resto1 < 2 ? 0 : 11 - resto1;

        if (numeros[12] != dig1)
            return false;

        var soma2 = 0;
        for (var i = 0; i < 13; i++)
            soma2 += numeros[i] * _multiplicadores2[i];

        var resto2 = soma2 % 11;
        var dig2 = resto2 < 2 ? 0 : 11 - resto2;

        return numeros[13] == dig2;
    }

    public bool Equals(Cnpj? other) => other is not null && Numero == other.Numero;
    public override bool Equals(object? obj) => Equals(obj as Cnpj);
    public override int GetHashCode() => Numero.GetHashCode();
}
