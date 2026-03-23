namespace CarStoreManager.Domain.ValueObjects;

public class Ano
{
    public int Valor { get; }

    public Ano(int valor)
    {
        var anoAtual = DateTime.Now.Year;

        if (valor < 1900 || valor > anoAtual)
            throw new ArgumentException("Ano inválido");

        Valor = valor;
    }
}