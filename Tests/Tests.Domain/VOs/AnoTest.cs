using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Domain.ValueObjects;

public class AnoTest
{
    [Fact]
    public void Deve_Criar_Ano_Valido()
    {
        var ano = new Ano(2013);
        ano.Valor.Should().Be(2013);
    }

    [Fact]
    public void Deve_Aceitar_Ano_1900()
    {
        var ano = new Ano(1900);
        ano.Valor.Should().Be(1900);
    }

    [Fact]
    public void Deve_Aceitar_Ano_Atual()
    {
        var anoAtual = DateTime.Now.Year;
        var ano = new Ano(anoAtual);

        ano.Valor.Should().Be(anoAtual);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(1500)]
    public void Deve_Rejeitar_Anos_Muito_Antigos(int valor)
    {
        Action act = () => new Ano(valor);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deve_Regeitar_Ano_Futuro()
    {
        var anoFuturo = DateTime.Now.Year + 1;

        Action act = () => new Ano(anoFuturo);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Ano inválido");
    }
}