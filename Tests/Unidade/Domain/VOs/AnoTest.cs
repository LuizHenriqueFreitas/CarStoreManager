using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Ano.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    testes implementados fazem:
        Verificar ano 1900 como valido,
        Verificar ano atual como valido,
        Bloquear ano 1899 ou inferiores,
        Bloquear ano autal + 1 ou superiores.
*/

public class AnoTest
{
    [Fact]
    public void Valida_Limite_De_Ano_Atual()
    {
        //cenario
        var ano = new Ano(DateTime.Now.Year);

        //aplicação
        bool anoValido = ano.ValidaAno(ano.GetValorAno());

        //validação
        Assert.True(anoValido);
    }

    [Fact]
    public void Valida_Limite_De_Ano_1900()
    {
        //cenario
        var ano = new Ano(1900);

        //aplicação
        bool anoValido = ano.ValidaAno(ano.GetValorAno());

        //validação
        Assert.True(anoValido);
    }

    /*
        No próprio metodo construtor do ano, ele chama 
        a função de validação, sendo assim, nos testes abaixo
        a criação do cenario, de criar um ano já é mandada como
        parametro para o assert que deve receber verifica a 
        mensagem de erro presente na exception "AnoInvalidoException"
    */

    [Fact]
    public void Deve_Negar_Ano_Muito_Antigo()
    {
        Assert.Throws<AnoInvalidoException>(() => new Ano(1899));
    }

    [Fact]
    public void Deve_Negar_Ano_Futuro()
    {
        var anoFuturo = DateTime.Now.Year + 1;

        Assert.Throws<AnoInvalidoException>(() => new Ano(anoFuturo));
    }
}