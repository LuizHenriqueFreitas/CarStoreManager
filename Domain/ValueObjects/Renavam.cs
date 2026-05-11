using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de RENAVAM (Registro Nacional de Veículos Automotores).
    Aceita 9 a 11 dígitos (RENAVAMs antigos podem ter menos de 11);
    armazena sempre normalizado para 11 dígitos (zero-padded à esquerda).
    Valida o dígito verificador (algoritmo módulo 11).
*/
public sealed class Renavam : IEquatable<Renavam>
{
    public string Numero { get; } = null!;

    protected Renavam() { }

    public Renavam(string renavam)
    {
        if (!Validar(renavam))
            throw new RenavamInvalidoException(renavam ?? string.Empty);

        Numero = Limpar(renavam!).PadLeft(11, '0');
    }

    public string GetNumeroRenavam() => Numero;

    public override string ToString() => Numero;

    private static string Limpar(string renavam)
        => new string(renavam.Where(char.IsDigit).ToArray());

    public static bool Validar(string? renavam)
    {
        if (string.IsNullOrWhiteSpace(renavam))
            return false;

        var limpo = Limpar(renavam);

        if (limpo.Length < 9 || limpo.Length > 11)
            return false;

        // Normaliza para 11 dígitos com zero à esquerda.
        limpo = limpo.PadLeft(11, '0');

        var dezDigitos = limpo.Substring(0, 10);
        var dvInformado = int.Parse(limpo.Substring(10, 1));

        // Reverte os 10 primeiros dígitos para multiplicar por 2..3..4..5..6..7..8..9..2..3
        var invertido = new string(dezDigitos.Reverse().ToArray());
        int[] multiplicadores = { 2, 3, 4, 5, 6, 7, 8, 9, 2, 3 };

        var soma = 0;
        for (var i = 0; i < 10; i++)
            soma += int.Parse(invertido[i].ToString()) * multiplicadores[i];

        var mod = (soma * 10) % 11;
        var dvCalculado = mod == 10 ? 0 : mod;

        return dvCalculado == dvInformado;
    }

    public bool Equals(Renavam? other) => other is not null && Numero == other.Numero;
    public override bool Equals(object? obj) => Equals(obj as Renavam);
    public override int GetHashCode() => Numero.GetHashCode();
}
