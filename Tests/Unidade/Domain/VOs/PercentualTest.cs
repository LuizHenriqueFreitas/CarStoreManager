using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.ValueObjects;

/*
    Este arquivo implementa os testes automaticos da classe Percentual.cs
    um VO do sistema, que inclui regra internas de funcionamento.

    Testes automaticos implementados para:
        Criar percentual valido,
        Bloquear percentuais Invalidos,
        Valida apresentação do valor do desconto,
        Valida apresentação do desconto em percentual,
        Valida Atualização do desconto
*/
public class PercentualTest
{
    //testa os limites permitidos
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Criar_Percentual_Valido(decimal valor)
    {
        var porcentagem = new Percentual(valor);

        Assert.True(porcentagem.GetDescontoValor() == valor);
    }

    //testa os limites bloqueados
    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Bloquear_Percentual_Invalido(decimal valor)
    {
        Assert.Throws<ArgumentException>(() => new Percentual(valor));
    }

    //apresenta o valor descontado do valor base
    [Fact]
    public void Deve_Apresentar_Valor_Do_Desconto()
    {
        // Given
        var valorBase = new Dinheiro(2000);
        var desconto = new Percentual(25);

        // When
        var valor = desconto.CalcularDescontoValor(valorBase);

        // Then
        Assert.True(valor.GetValorDinheiro() == 500);
    }

    //apresenta o desconto em %
    [Fact]
    public void Deve_Apresentar_Desconto_Percentual()
    {
        // Given
        var desconto = new Percentual(25);

        // Then
        Assert.True(desconto.GetDescontoPercentual() == "25%");
    }

    //atualiza o valor da porcentagem
    [Fact]
    public void Deve_Atualizar_Desconto()
    {
        // Given
        var desconto = new Percentual(25);

        // When
        desconto.AtualizarPercentual(30);

        // Then
        Assert.True(desconto.GetDescontoPercentual() == "30%");
    }
}