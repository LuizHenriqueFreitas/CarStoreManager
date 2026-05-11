using FluentAssertions;
using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.VOs;

public class RenavamTest
{
    [Theory]
    [InlineData("12345678900")] // 11 dígitos com DV correto
    [InlineData("00000678902")] // pré-calculado
    public void Construtor_RenavamValido_NaoLanca(string renavam)
    {
        Action act = () => new Renavam(renavam);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]      // poucos dígitos
    [InlineData("123456789012")] // mais dígitos
    [InlineData("12345678901")] // DV errado
    [InlineData("abcdefgh123")]
    [InlineData(null)]
    public void Construtor_RenavamInvalido_Lanca(string? renavam)
    {
        Action act = () => new Renavam(renavam!);
        act.Should().Throw<RenavamInvalidoException>();
    }

    [Fact]
    public void Construtor_NormalizaPara11DigitosComZeroAEsquerda()
    {
        // RENAVAM com 10 dígitos válido (0000678902 → DV 2 quando completado p/ 11)
        var renavam = new Renavam("0000678902");
        renavam.Numero.Length.Should().Be(11);
        renavam.Numero.Should().StartWith("0");
    }

    [Fact]
    public void Construtor_RemovePontuacao()
    {
        Action act = () => new Renavam("123-456-789-00");
        act.Should().NotThrow();
    }

    [Fact]
    public void Equals_MesmoNumero_RetornaTrue()
    {
        var a = new Renavam("12345678900");
        var b = new Renavam("12345678900");
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Validar_Null_RetornaFalse()
        => Renavam.Validar(null).Should().BeFalse();

    [Fact]
    public void Validar_Vazio_RetornaFalse()
        => Renavam.Validar("").Should().BeFalse();
}
