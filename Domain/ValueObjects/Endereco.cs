namespace CarStoreManager.Domain.ValueObjects;

/*
    VO de Endereço residencial do cliente.
    Composto pelos campos: Logradouro, Numero, Complemento (opcional),
    Bairro, Cidade, UF e CEP.

    Validações mínimas para não inserir lixo no banco. Complemento é
    o único campo opcional. CEP é normalizado para apenas dígitos
    (8 caracteres).
*/
public class Endereco
{
    public string Logradouro { get; private set; } = null!;
    public string Numero { get; private set; } = null!;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = null!;
    public string Cidade { get; private set; } = null!;
    public string Uf { get; private set; } = null!;
    public string Cep { get; private set; } = null!;

    protected Endereco() { }

    public Endereco(
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        string cep)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            throw new ArgumentException("Logradouro é obrigatório");
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número é obrigatório");
        if (string.IsNullOrWhiteSpace(bairro))
            throw new ArgumentException("Bairro é obrigatório");
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade é obrigatória");
        if (string.IsNullOrWhiteSpace(uf) || uf.Trim().Length != 2)
            throw new ArgumentException("UF deve ter 2 caracteres");

        var cepDigits = new string((cep ?? "").Where(char.IsDigit).ToArray());
        if (cepDigits.Length != 8)
            throw new ArgumentException("CEP inválido (deve conter 8 dígitos)");

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
        Cep = cepDigits;
    }

    public string CepFormatado()
        => $"{Cep.Substring(0, 5)}-{Cep.Substring(5, 3)}";

    public override string ToString()
    {
        var parteNumero = string.IsNullOrWhiteSpace(Complemento)
            ? Numero
            : $"{Numero} - {Complemento}";

        return $"{Logradouro}, {parteNumero} · {Bairro} · {Cidade}/{Uf} · CEP {CepFormatado()}";
    }
}
