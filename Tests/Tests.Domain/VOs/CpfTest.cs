using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Domain.ValueObjects;

public class CpfTest
{
    [Fact]
    public void Deve_Criar_Cpf_Valido()
    {
        var cpf = new Cpf("529.982.247-25");

        cpf.Numero.Should().Be("52998224725");
    }

    [Fact]
    public void Deve_Lancar_Excecao_Para_Cpf_Invalido()
    {
        Action act = () => new Cpf("111.111.111-11");

        act.Should().Throw<ArgumentException>()
        .WithMessage("CPF inválido");
    }

    [Fact]
    public void Deve_Remover_Formatacao()
    {
        var cpf = new Cpf("529.982.247-25");

        cpf.Numero.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("52998224725")]
    [InlineData("12345678909")]
    public void Deve_Validar_Cpf_Corretamente(string numero)
    {
        Cpf.Validar(numero).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("52998224724")] // dígito errado
    public void Deve_Rejeitar_Cpf_Invalido(string numero)
    {
        Cpf.Validar(numero).Should().BeFalse();
    }

    [Fact]
    public void Deve_Formatar_Cpf_Corretamente()
    {
        var cpf = new Cpf("52998224725");

        cpf.ToString().Should().Be("529.982.247-25");
    }
}
