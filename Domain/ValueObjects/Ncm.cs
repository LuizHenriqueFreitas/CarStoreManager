using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de NCM (Nomenclatura Comum do Mercosul) — classificação fiscal de produtos.
    Sempre 8 dígitos. Obrigatório em notas fiscais brasileiras.
    Formato canônico de exibição: XXXX.XX.XX
*/
public sealed class Ncm : IEquatable<Ncm>
{
    public string Valor { get; }

    protected Ncm() { Valor = null!; }

    public Ncm(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new NcmInvalidoException(valor ?? string.Empty);

        var limpo = valor.Replace(".", "").Trim();

        if (limpo.Length != 8 || !limpo.All(char.IsDigit))
            throw new NcmInvalidoException(valor);

        Valor = limpo;
    }

    public string Formatado()
        => $"{Valor.Substring(0, 4)}.{Valor.Substring(4, 2)}.{Valor.Substring(6, 2)}";

    public override string ToString() => Valor;
    public bool Equals(Ncm? other) => other is not null && Valor == other.Valor;
    public override bool Equals(object? obj) => Equals(obj as Ncm);
    public override int GetHashCode() => Valor.GetHashCode();

    public static implicit operator string(Ncm ncm) => ncm.Valor;
}
