using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Domain.ValueObjects;

public class FinanciamentoTest
{
    [Fact]
    public void Deve_Criar_Financiamento_Valido()
    {
        var total = new Dinheiro(10000);
        var entrada = new Dinheiro(2000);
        var parcelas = new Parcelas(10);

        var financiamento = new Financiamento(total, parcelas, entrada);

        financiamento.ValorTotal.Should().Be(total);
        financiamento.Entrada.Should().Be(entrada);
        financiamento.Parcelas.Should().Be(parcelas);
    }

    [Fact]
    public void Deve_Rejeitar_Entrada_Maior_Que_Total()
    {
        var total = new Dinheiro(1000);
        var entrada = new Dinheiro(2000);
        var parcelas = new Parcelas(10);

        Action act = () => new Financiamento(total, parcelas, entrada);

        act.Should().Throw<ArgumentException>()
        .WithMessage("Entrada não pode ser maior que o valor total");
    }
    
    [Fact]
    public void Deve_Calcular_Valor_Financiado()
    {
        var financiamento = new Financiamento(
            new Dinheiro(10000),
            new Parcelas(10),
            new Dinheiro(2000)
        );

        financiamento.ValorFinanciado.Valor.Should().Be(8000);
    }

    [Fact]
    public void Deve_Calcular_Valor_Da_Parcela()
    {
        var financiamento = new Financiamento(
            new Dinheiro(10000),
            new Parcelas(10),
            new Dinheiro(2000)
        );

        financiamento.ValorParcela.Valor.Should().Be(800);
    }

    [Fact]
    public void Deve_Arredondar_Valor_Da_Parcela()
    {
        var financiamento = new Financiamento(
            new Dinheiro(1000),
            new Parcelas(3),
            new Dinheiro(0)
        );

        financiamento.ValorParcela.Valor.Should().Be(333.33m);
    }

    [Fact]
    public void Deve_Suportar_Entrada_Zero()
    {
        var financiamento = new Financiamento(
            new Dinheiro(1000),
            new Parcelas(5),
            new Dinheiro(0)
        );

        financiamento.ValorFinanciado.Valor.Should().Be(1000);
    }

    [Fact]
    public void Deve_Suportar_Entrada_Igual_Ao_Total()
    {
        var financiamento = new Financiamento(
            new Dinheiro(1000),
            new Parcelas(5),
            new Dinheiro(1000)
        );

        financiamento.ValorFinanciado.Valor.Should().Be(0);
        financiamento.ValorParcela.Valor.Should().Be(0);
    }
}