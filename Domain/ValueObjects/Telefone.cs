using System.Text.RegularExpressions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Telefone, alem dos metodos 
    get e set.

    Testes automaticos implementados para:
        Criar telefones validos,
        Bloquear telefones invalidos,
        Permitir Atualização de numero valido,
        Bloquear Atualização de numero invalido,
        Verifivar a formatação de saida com o metodo ToString()
*/

public class Telefone
{
    private string Numero { get; set; } = null!;

    protected Telefone() { }

    //construtor com validação
    public Telefone(string numero)
    {
        AtualizarTelefone(numero);
    }

    //metodo get - pega o numero do telefone sem formatação
    public string GetTelefone()
    {
        return Numero;
    }

    //atualiza o numero atual por um novo
    public void AtualizarTelefone(string novoNumero)
    {
        if (string.IsNullOrWhiteSpace(novoNumero))
            throw new ArgumentException("Telefone não pode ser vazio");

        var apenasNumeros = NormalizaTelefone(novoNumero);

        if (!ValidaTelefone(apenasNumeros))
            throw new ArgumentException("Telefone inválido");

        Numero = apenasNumeros;
    }

    //normaliza a entrada adequadamente - retira tudo que não for um numero
    private string NormalizaTelefone(string input)
    {
        return Regex.Replace(input, @"\D", "");
    }

    /*
        valida a quantidade de caracteres do telefone
        no padrão brasileiro: DDD (2) + número (8 ou 9)
    */
    private bool ValidaTelefone(string numero)
    {
        return numero.Length is 10 or 11;
    }

    //sobre escreve a função ToString() para incluir a formatação correta
    public override string ToString()
    {
        if (Numero.Length == 11)
        {
            return $"({Numero[..2]}) {Numero[2..7]}-{Numero[7..]}";
        }

        return $"({Numero[..2]}) {Numero[2..6]}-{Numero[6..]}";
    }


    //funcoes utilitarias de bibliotecas inclusas
    public override bool Equals(object? obj)
    {
        if (obj is not Telefone other)
            return false;

        return Numero == other.Numero;
    }

    public override int GetHashCode()
        => Numero.GetHashCode();

    public static bool operator ==(Telefone a, Telefone b)
        => a?.Numero == b?.Numero;

    public static bool operator !=(Telefone a, Telefone b)
        => !(a == b);
}