using CarStoreManager.Domain.Exceptions;

namespace CarStoreManager.Domain.ValueObjects;

/*
    Neste arquivo temos as implementações de verificações 
    e regras de negócios do VO de ANO, alem dos metodos de:
    GetValorAno() e AtualizarAno() - servem como getter e setter
    respectivamente. 

    Validamos os limites validos de anos entre 1899 e o ano atual.

    Testes automaticos implementados para:
        Verificar ano 1900 como valido,
        Verificar ano atual como valido,
        Bloquear ano 1899 ou inferiores,
        Bloquear ano autal + 1 ou superiores.
*/

public class Ano
{
    private int Valor { get; set; }

    protected Ano () {}

    public Ano(int valor)
    {
        //valida o ano já na criação
        if (!ValidaAno(valor))
            throw new AnoInvalidoException(valor);
            
        Valor = valor;
    }

    //metodo de atualização - serve como um setter
    public void AtualizaAno(int novoAno)
    {
        //valida o ano na atualização também
        if (ValidaAno(novoAno))
            Valor = novoAno;
    }

    //metodo getter da variavel de valor do VO
    public int GetValorAno()
    {
        return Valor;
    }

    //função que valida se o Ano esta dentro das regras de negocio
    public bool ValidaAno(int ano)
    {
        var anoAtual = DateTime.Now.Year;

        if (ano < 1900 || ano > anoAtual)//verificação de anos permitidos
            return false;
            
        return true;
    }
}