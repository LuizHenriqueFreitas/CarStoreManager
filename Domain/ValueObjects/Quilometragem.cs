using System.Globalization;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de Quilometragem, 
    alem dos metodos get e set.

    Testes automaticos implementados para:
        Criar uma quilometragem valida,
        Bloquear a criação de quilometragem invalida,
        Valida atualização de quilometragem,
        Bloqueia deminuir uma quilometragem ja registrada,
        Valida formatação de saida do metodo ToString
*/

public class Quilometragem
{
    private int Valor { get; set; }

    protected Quilometragem() { }

    //construtor valida a entrada
    public Quilometragem(int valor)
    {
        if (!ValidaQuilometragem(valor, 0))
            throw new ArgumentException("Quilometragem não pode ser negativa");

        Valor = valor;
    }

    //metodo getter que pega o valor da quilometrafgem real como inteiro
    public int GetQuilometragem()
    {
        return Valor;
    }

    //metodo de saida ja tras uma string formatada
    public override string ToString()
    {
        return $"{Valor.ToString("N0", new CultureInfo("pt-BR"))} Km";
    }

    //metodo que serve como setter, valida os novos valores
    public void AtualizarQuilometragem(int novoValor)
    {
        if (!ValidaQuilometragem(novoValor, Valor))
            throw new InvalidOperationException("Quilometragem não pode diminuir");

        Valor = novoValor;
    }

    /*
        metodo que verifica a Quilometragem
        A regra adotada é que um carro não perde
        Quilometragem, sendo assim a nova nunca pode
        ser menor que a anterior
    */
    public bool ValidaQuilometragem(int valor, int limite)
    {
        if(valor < limite)
            return false;
        
        return true;
    }

    //funcoes utilitarias de bibliotecas inclusas
    public override bool Equals(object? obj)
    {
        if (obj is not Quilometragem other)
            return false;

        return Valor == other.Valor;
    }

    public override int GetHashCode()
        => Valor.GetHashCode();

    public static bool operator ==(Quilometragem a, Quilometragem b)
        => a?.Valor == b?.Valor;

    public static bool operator !=(Quilometragem a, Quilometragem b)
        => !(a == b);
}