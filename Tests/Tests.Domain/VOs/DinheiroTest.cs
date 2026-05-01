using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Domain.ValueObjects;

public class DinheiroTest
{
    [Fact]
    public void Deve_Criar_Dinheiro_Valido()
    {
        var dinheiro = new Dinheiro(10.50m);

        dinheiro.Valor.Should().Be(10.50m);
    }

    [Fact]
    public void Deve_Arredondar_Para_Duas_Casas()
    {
        var dinheiro = new Dinheiro(10.555m);

        dinheiro.Valor.Should().Be(10.56m);
    }

    [Fact]
    public void Deve_Rejeitar_Valor_Negativo()
    {
        Action act = () => new Dinheiro(-1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deve_Somar_Valores()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(5);

        var resultado = a.Somar(b);

        resultado.Valor.Should().Be(15);
    }

    [Fact]
    public void Deve_Somar_Usando_Operador()
    {
        var resultado = new Dinheiro(10) + new Dinheiro(5);

        resultado.Valor.Should().Be(15);
    }

    [Fact]
    public void Deve_Subtrair_Valores()
    {
        var resultado = new Dinheiro(10).Subtrair(new Dinheiro(5));

        resultado.Valor.Should().Be(5);
    }

    [Fact]
    public void Deve_Nao_Permitir_Resultado_Negativo()
    {
        Action act = () => new Dinheiro(5).Subtrair(new Dinheiro(10));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deve_Multiplicar()
    {
        var resultado = new Dinheiro(10).Multiplicar(3);

        resultado.Valor.Should().Be(30);
    }

    [Fact]
    public void Deve_Rejeitar_Quantidade_Negativa()
    {
        Action act = () => new Dinheiro(10).Multiplicar(-1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deve_Dividir()
    {
        var resultado = new Dinheiro(10).Dividir(2);

        resultado.Valor.Should().Be(5);
    }

    [Fact]
    public void Deve_Rejeitar_Divisor_Zero()
    {
        Action act = () => new Dinheiro(10).Dividir(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deve_Ser_Maior_Que()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(5);

        a.MaiorQue(b).Should().BeTrue();
    }

    [Fact]
    public void Deve_Suportar_Operador_Maior()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(5);

        (a > b).Should().BeTrue();
    }

    [Fact]
    public void Deve_Ser_Igual_Por_Valor()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(10);

        a.Should().Be(b);
    }

    [Fact]
    public void Deve_Diferenciar_Valores()
    {
        var a = new Dinheiro(10);
        var b = new Dinheiro(5);

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void Deve_Formatar_Corretamente()
    {
        var dinheiro = new Dinheiro(10);

        dinheiro.ToString().Should().Be("R$ 10,00");
    }
}