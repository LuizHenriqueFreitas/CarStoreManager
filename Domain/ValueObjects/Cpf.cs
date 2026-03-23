namespace CarStoreManager.Domain.ValueObjects;

public class Cpf
{
    public string Numero { get; }

    public Cpf(string numero)
    {
        if (!Validar(numero))
            throw new ArgumentException("CPF inválido");

        Numero = Limpar(numero);
    }

    private static string Limpar(string cpf)
    {
        return cpf.Replace(".", "")
                  .Replace("-", "")
                  .Trim();
    }

    public static bool Validar(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = Limpar(cpf);

        if (cpf.Length != 11)
            return false;

        if (cpf.All(c => c == cpf[0]))
            return false;

        int[] numeros = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        if (numeros[9] != digito1)
            return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return numeros[10] == digito2;
    }

    public override string ToString()
    {
        return Convert.ToUInt64(Numero).ToString(@"000\.000\.000\-00");
    }
}
