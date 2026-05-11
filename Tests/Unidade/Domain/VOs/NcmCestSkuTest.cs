using FluentAssertions;
using CarStoreManager.Domain.Exceptions;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Domain.VOs;

public class NcmTest
{
    [Theory]
    [InlineData("87083010")]
    [InlineData("8708.30.10")]
    public void Construtor_NcmValido_NaoLanca(string ncm)
    {
        Action act = () => new Ncm(ncm);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("123456789")]
    [InlineData("abcdefgh")]
    public void Construtor_NcmInvalido_Lanca(string ncm)
    {
        Action act = () => new Ncm(ncm);
        act.Should().Throw<NcmInvalidoException>();
    }

    [Fact]
    public void Formatado_RetornaPadraoXxxxXxXx()
    {
        var ncm = new Ncm("87083010");
        ncm.Formatado().Should().Be("8708.30.10");
    }
}

public class CestTest
{
    [Theory]
    [InlineData("0102000")]
    [InlineData("01.020.00")]
    public void Construtor_CestValido_NaoLanca(string cest)
    {
        Action act = () => new Cest(cest);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345678")]
    [InlineData("abcdefg")]
    public void Construtor_CestInvalido_Lanca(string cest)
    {
        Action act = () => new Cest(cest);
        act.Should().Throw<CestInvalidoException>();
    }

    [Fact]
    public void Construtor_VazioOuNull_AceitaComoOpcional()
    {
        new Cest(null).EstaPreenchido.Should().BeFalse();
        new Cest("").EstaPreenchido.Should().BeFalse();
    }
}

public class SkuTest
{
    [Theory]
    [InlineData("ABC-123")]
    [InlineData("PFD/001")]
    [InlineData("xyz.99")]
    public void Construtor_SkuValido_NaoLanca(string sku)
    {
        Action act = () => new Sku(sku);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC@123")]      // @ não é permitido
    public void Construtor_SkuInvalido_Lanca(string sku)
    {
        Action act = () => new Sku(sku);
        act.Should().Throw<SkuInvalidoException>();
    }

    [Fact]
    public void Construtor_NormalizaParaMaiusculas()
    {
        var sku = new Sku("abc-001");
        sku.Valor.Should().Be("ABC-001");
    }
}

public class PartNumberTest
{
    [Fact]
    public void Construtor_NormalizaParaMaiusculas()
    {
        var pn = new PartNumber("pn-12345");
        pn.Valor.Should().Be("PN-12345");
    }

    [Fact]
    public void Construtor_VazioLanca()
    {
        Action act = () => new PartNumber("");
        act.Should().Throw<PartNumberInvalidoException>();
    }
}

public class CodigoOemTest
{
    [Fact]
    public void Construtor_VazioOuNull_AceitaComoOpcional()
    {
        new CodigoOEM(null).EstaPreenchido.Should().BeFalse();
        new CodigoOEM("").EstaPreenchido.Should().BeFalse();
    }

    [Fact]
    public void Construtor_FormatoInvalido_Lanca()
    {
        Action act = () => new CodigoOEM("abc 123");
        act.Should().Throw<CodigoOEMInvalidoException>();
    }
}

public class CodigoBarrasTest
{
    [Theory]
    [InlineData("12345678")]        // EAN-8
    [InlineData("123456789012")]    // UPC-A
    [InlineData("1234567890123")]   // EAN-13
    [InlineData("12345678901234")]  // GTIN-14
    public void Construtor_ComprimentosValidos_NaoLanca(string codigo)
    {
        Action act = () => new CodigoBarras(codigo);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("1234567")]         // 7 dígitos
    [InlineData("123456789")]       // 9 dígitos
    [InlineData("abc12345")]
    public void Construtor_ComprimentoOuConteudoInvalido_Lanca(string codigo)
    {
        Action act = () => new CodigoBarras(codigo);
        act.Should().Throw<CodigoBarrasInvalidoException>();
    }

    [Fact]
    public void Construtor_VazioOuNull_AceitaComoOpcional()
    {
        new CodigoBarras(null).EstaPreenchido.Should().BeFalse();
        new CodigoBarras("").EstaPreenchido.Should().BeFalse();
    }
}
