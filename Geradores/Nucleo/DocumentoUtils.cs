namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Gera documentos e códigos brasileiros que passam nas validações estritas
/// dos Value Objects do domínio (dígito verificador de CPF/RENAVAM, formato
/// de placa, telefone com DDD). Centralizado aqui para ser reaproveitado por
/// qualquer gerador de entidade que precise desses campos.
/// </summary>
public static class DocumentoUtils
{
    private static readonly string[] Letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => c.ToString()).ToArray();
    private static readonly int[] DddsValidos =
    {
        11, 12, 13, 14, 15, 16, 17, 18, 19, // SP
        21, 22, 24, // RJ
        27, 28, // ES
        31, 32, 33, 34, 35, 37, 38, // MG
        41, 42, 43, 44, 45, 46, // PR
        47, 48, 49, // SC
        51, 53, 54, 55, // RS
        61, 62, 63, 64, 65, 66, 67, 68, 69, // Centro-Oeste/Norte
        71, 73, 74, 75, 77, 79, // BA/SE
        81, 82, 83, 84, 85, 86, 87, 88, 89, // Nordeste
        91, 92, 93, 94, 95, 96, 97, 98, 99 // Norte
    };

    /// <summary>Gera um CPF válido (dígitos verificadores corretos) a partir de uma base pseudo-aleatória.</summary>
    public static string GerarCpf(Random rng)
    {
        int[] baseNumeros;
        do
        {
            baseNumeros = Enumerable.Range(0, 9).Select(_ => rng.Next(0, 10)).ToArray();
        } while (baseNumeros.Distinct().Count() == 1); // rejeita "111111111" etc.

        var d1 = CalcularDigitoCpf(baseNumeros, 10);
        var com9 = baseNumeros.Append(d1).ToArray();
        var d2 = CalcularDigitoCpf(com9, 11);

        var todos = com9.Append(d2).ToArray();
        return string.Concat(todos);
    }

    private static int CalcularDigitoCpf(int[] numeros, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < numeros.Length; i++)
            soma += numeros[i] * (pesoInicial - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>Gera um RENAVAM válido (11 dígitos, dígito verificador módulo 11 correto).</summary>
    public static string GerarRenavam(Random rng)
    {
        var dezDigitos = string.Concat(Enumerable.Range(0, 10).Select(_ => rng.Next(0, 10)));

        var invertido = new string(dezDigitos.Reverse().ToArray());
        int[] multiplicadores = { 2, 3, 4, 5, 6, 7, 8, 9, 2, 3 };

        var soma = 0;
        for (var i = 0; i < 10; i++)
            soma += int.Parse(invertido[i].ToString()) * multiplicadores[i];

        var mod = (soma * 10) % 11;
        var dv = mod == 10 ? 0 : mod;

        return dezDigitos + dv;
    }

    /// <summary>Gera uma placa no formato antigo brasileiro (ABC1234).</summary>
    public static string GerarPlaca(Random rng)
    {
        var letras = string.Concat(Enumerable.Range(0, 3).Select(_ => Letras[rng.Next(Letras.Length)]));
        var numeros = rng.Next(0, 10000).ToString("D4");
        return $"{letras}{numeros}";
    }

    /// <summary>Gera um telefone celular válido: DDD real + 9 + 8 dígitos (11 dígitos totais).</summary>
    public static string GerarTelefoneCelular(Random rng)
    {
        var ddd = DddsValidos[rng.Next(DddsValidos.Length)];
        var numero = rng.Next(0, 100_000_000).ToString("D8");
        return $"{ddd}9{numero}";
    }

    /// <summary>Gera 8 dígitos numéricos aleatórios (usado como NCM — sem regra de dígito verificador).</summary>
    public static string GerarDigitos(Random rng, int quantidade)
        => string.Concat(Enumerable.Range(0, quantidade).Select(_ => rng.Next(0, 10)));

    /// <summary>Gera um CEP (8 dígitos) plausível.</summary>
    public static string GerarCep(Random rng)
        => GerarDigitos(rng, 8);

    /// <summary>
    /// Sorteia um valor "redondo" no intervalo [min, max], arredondado para o
    /// múltiplo indicado — evita centavos/quebrados tipo "47238.63" numa base
    /// de teste, onde valores tipo "47000" ou "47500" facilitam a leitura dos
    /// gráficos/relatórios sem exigir precisão realista de centavos.
    /// </summary>
    public static decimal ValorRedondo(Random rng, int min, int max, int multiplo)
    {
        var bruto = rng.Next(min, max + 1);
        var arredondado = (int)Math.Round(bruto / (double)multiplo) * multiplo;
        return Math.Max(arredondado, multiplo);
    }
}
