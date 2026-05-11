using FluentAssertions;
using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.VOs;

public class CnpjTest
{
    [Theory]
    [InlineData("11.222.333/0001-81")]      // CNPJ válido conhecido
    [InlineData("11222333000181")]          // mesmo, sem formatação
    public void Construtor_CnpjValido_NaoLanca(string cnpj)
    {
        Action act = () => new Cnpj(cnpj);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("11111111111111")]      // todos iguais
    [InlineData("11222333000182")]      // DV errado
    [InlineData("aaaaaaaaaaaaa")]
    [InlineData(null)]
    public void Construtor_CnpjInvalido_LancaCnpjInvalidoException(string? cnpj)
    {
        Action act = () => new Cnpj(cnpj!);
        act.Should().Throw<CnpjInvalidoException>();
    }

    [Fact]
    public void Construtor_ArmazenaSemFormatacao()
    {
        var cnpj = new Cnpj("11.222.333/0001-81");
        cnpj.Numero.Should().Be("11222333000181");
    }

    [Fact]
    public void ToString_RetornaFormatado()
    {
        var cnpj = new Cnpj("11222333000181");
        cnpj.ToString().Should().Be("11.222.333/0001-81");
    }

    [Fact]
    public void Equals_MesmoNumero_RetornaTrue()
    {
        var a = new Cnpj("11222333000181");
        var b = new Cnpj("11.222.333/0001-81");
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
