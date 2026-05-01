using CarStoreManager.Domain.ValueObjects;
using FluentAssertions;

namespace CarStoreManager.Tests.Domain.ValueObjects;

public class EmailTest
{
    [Fact]
    public void Deve_Criar_Email_Valido()
    {
        var email = new Email("teste@email.com");

        email.Endereco.Should().Be("teste@email.com");
    }

    [Fact]
    public void Deve_Normalizar_Email()
    {
        var email = new Email("  TESTE@Email.COM ");

        email.Endereco.Should().Be("teste@email.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Rejeitar_Email_Vazio(string valor)
    {
        Action act = () => new Email(valor);

        act.Should().Throw<ArgumentException>()
        .WithMessage("Email não pode ser vazio");
    }

    [Theory]
    [InlineData("teste")]
    [InlineData("teste@")]
    [InlineData("@email.com")]
    [InlineData("teste@email")]
    [InlineData("teste@.com")]
    public void Deve_Rejeitar_Email_Invalido(string valor)
    {
        Action act = () => new Email(valor);

        act.Should().Throw<ArgumentException>()
        .WithMessage("Email inválido");
    }

    [Fact]
    public void Deve_Ser_Igual_Independentemente_De_Case()
    {
        var a = new Email("TESTE@email.com");
        var b = new Email("teste@email.com");

        a.Should().Be(b);
    }

    [Fact]
    public void Deve_Comparar_Com_Operadores()
    {
        var a = new Email("teste@email.com");
        var b = new Email("teste@email.com");

        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Deve_Ter_Mesmo_HashCode_Para_Emails_Iguais()
    {
        var a = new Email("teste@email.com");
        var b = new Email("TESTE@email.com");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Deve_Retornar_Endereco()
    {
        var email = new Email("teste@email.com");

        email.ToString().Should().Be("teste@email.com");
    }
}