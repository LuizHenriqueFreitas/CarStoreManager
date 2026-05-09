using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Financiamento.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar Financiamento Valido,
        Atualizar um financiamento valido,
        Verificar o calculo de valor financiado,
        Verificar o calculo de valor da parcela,
        Verificar que o valor da parcela é arredondado,
        Bloquear entrada menor que 500,
        Bloquear entrada maior que o total,
        Bloquear entrada igual ao total
*/

public class FinanciamentoTest
{
    //criar financiamento valido
    [Fact]
    public void Deve_Criar_Financiamento_Valido()
    {
        var financiamento = new Financiamento(10000, 10, 2000);

        Assert.True(financiamento.GetValorTotal() == 10000);
        Assert.True(financiamento.GetValorEntrada() == 2000);
        Assert.True(financiamento.GetQuantidadeParcelas() == 10);
    }

    //atualizar financiamento valido
    [Fact]
    public void Deve_Atualizar_Financiamento_Valido()
    {
        var financiamento = new Financiamento(10000, 10, 2000);

        financiamento.AtualizarFinanciamento(12000, 8, 2500);

        Assert.True(financiamento.GetValorTotal() == 12000);
        Assert.True(financiamento.GetValorEntrada() == 2500);
        Assert.True(financiamento.GetQuantidadeParcelas() == 8);
    }
    
    //verifica o calculo do valor financiado
    [Fact]
    public void Deve_Calcular_Valor_Financiado()
    {
        var financiamento = new Financiamento(
            10000,
            10,
            2000
        );

        Assert.True(financiamento.GetValorFinanciado() == 8000);
    }

    //verifica o calculo do valor da parcela
    [Fact]
    public void Deve_Calcular_Valor_Da_Parcela()
    {
        var financiamento = new Financiamento(
            10000,
            10,
            2000
        );

        Assert.True(financiamento.GetValorParcela() == 800);
    }

    //varifica se o valor da parcela é arredondado a 2 casas decimais, como deve ser
    [Fact]
    public void Deve_Arredondar_Valor_Da_Parcela()
    {
        var financiamento = new Financiamento(
            15000,
            3,
            5000
        );

        Assert.True(financiamento.GetValorParcela() == 3333.33m);
    }

    //bloqueia entrada menor que 500
    [Fact]
    public void Deve_Bloquear_Entrada_Menor_Que_500()
    {
        Assert.Throws<ArgumentException>(() => new Financiamento(
            1000,
            5,
            499.9m
        ));
    }

    //bloqueia entrada maior que o valor total
    [Fact]
    public void Deve_Bloquear_Entrada_Maior_Que_Total()
    {
        Assert.Throws<ArgumentException>(() => new Financiamento(
            1000,
            5,
            1000.1m
        ));
    }

    //bloqueia entrada igual ao valor total
    [Fact]
    public void Deve_Bloquear_Entrada_Igual_Ao_Total()
    {
        Assert.Throws<ArgumentException>(() => new Financiamento(
            1000,
            5,
            1000
        ));
    }
}