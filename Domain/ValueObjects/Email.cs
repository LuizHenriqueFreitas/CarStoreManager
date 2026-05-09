using System.Text.RegularExpressions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Email, alem dos metodos de:
    GetEmail() e SetEmail().

    Testes automaticos implementados para:
        Criar um email valido,
        Verificar a normalização do email,
        Bloquear email vazio,
        Bloquear email com regex errado,
        Verifica que emails sejam independentede to case sensitive,
        Verifica que emails sejam comparaveis com operadores == e !=
        Emails iguais devem ter o mesmo hashcode,
        Verificação do retorno com GetEmail
*/


public class Email : IEquatable<Email>
{
    public string Endereco { get; private set; } = null!;

    //declaração da formatação do regex do email
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    protected Email () {}

    /*
        O metodo construtor ja verifica se não é vazio 
        e se esta de acordo com o padrão Regex do email
    */
    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("Email não pode ser vazio");

        endereco = endereco.Trim().ToLower();

        if (!EmailRegex.IsMatch(endereco))
            throw new ArgumentException("Email inválido");

        Endereco = endereco;
    }

    public string GetEmail()
    {
        return Endereco.ToString();
    }

    /*
        O metodo set faz a verificação do email
        utilizando o metodo ValidaEmail()
    */
    public void SetEmail(string novoEmail)
    {
        if(!ValidaEmail(novoEmail))
            throw new ArgumentException("Email inválido");
        Endereco = novoEmail;
    }

    /*
        funcao que verifica se o email não é vazio e 
        se esta de acordo com o Regex do email
    */
    public bool ValidaEmail(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            return false;

        endereco = endereco.Trim().ToLower();

        if (!EmailRegex.IsMatch(endereco))
            return false;
        
        return true;
    }

    //funcoes utilitarias de bibliotecas inclusas

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Endereco == other.Endereco;
    }

    public override bool Equals(object? obj)
        => obj is Email other && Equals(other);

    public override int GetHashCode()
        => Endereco.GetHashCode();

    public static bool operator ==(Email a, Email b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Email a, Email b)
        => !Equals(a, b);
}