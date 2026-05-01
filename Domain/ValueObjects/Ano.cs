using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

public class Ano
{
    public int Valor { get; }

    protected Ano () {}

    public Ano(int valor)
    {
        if (ValidaAno(valor))
            Valor = valor;
    }

    public bool ValidaAno(int ano)
    {
        var anoAtual = DateTime.Now.Year;

        if (ano < 1900 || ano > anoAtual)
        {
            throw new AnoInvalidoException(ano);
        }
        return true;
    }
}