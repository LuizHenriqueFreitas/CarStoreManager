using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class ComponenteTest
{
    [Fact]
    public void Construtor_CamposValidos_AtribuiCorretamente()
    {
        var c = CriarComponenteValido();

        c.Nome.Should().Be("Pastilha de freio dianteira");
        c.SKUInterno.Should().Be("PFD-001");
        c.PartNumber.Should().Be("PN-12345");
        c.NCM.Should().Be("87083010");
        c.Categoria.Should().Be("Freios");
        c.Unidade.Should().Be("UN");
        c.Peso.Should().Be(0.500m);
        c.GarantiaDias.Should().Be(180);
        c.Ativo.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeInvalido_LancaArgumentException(string nome)
    {
        Action act = () => CriarComponente(nome: nome);
        act.Should().Throw<ArgumentException>().WithMessage("*Nome*");
    }

    [Fact]
    public void Construtor_NcmInvalido_LancaArgumentException()
    {
        Action act = () => CriarComponente(ncm: "1234");
        act.Should().Throw<ArgumentException>().WithMessage("*NCM*");
    }

    [Fact]
    public void Construtor_SkuVazio_LancaArgumentException()
    {
        Action act = () => CriarComponente(sku: "");
        act.Should().Throw<ArgumentException>().WithMessage("*SKU*");
    }

    [Fact]
    public void Construtor_PartNumberVazio_LancaArgumentException()
    {
        Action act = () => CriarComponente(partNumber: "");
        act.Should().Throw<ArgumentException>().WithMessage("*Part number*");
    }

    [Fact]
    public void Construtor_CodigoOEMVazio_AceitaComoOpcional()
    {
        var c = CriarComponente(codigoOEM: "");
        c.CodigoOEM.Should().Be(string.Empty);
    }

    [Fact]
    public void Construtor_CodigoBarrasVazio_AceitaComoOpcional()
    {
        var c = CriarComponente(codigoBarras: "");
        c.CodigoBarras.Should().Be(string.Empty);
    }

    [Fact]
    public void Construtor_CodigoBarrasComLetras_LancaArgumentException()
    {
        Action act = () => CriarComponente(codigoBarras: "abcdefgh");
        act.Should().Throw<ArgumentException>().WithMessage("*dígitos*");
    }

    [Fact]
    public void Construtor_CestVazio_AceitaComoOpcional()
    {
        var c = CriarComponente(cest: "");
        c.CEST.Should().Be(string.Empty);
    }

    [Fact]
    public void Construtor_CestComMenosDe7Digitos_LancaArgumentException()
    {
        Action act = () => CriarComponente(cest: "12345");
        act.Should().Throw<ArgumentException>().WithMessage("*CEST*");
    }

    [Fact]
    public void Construtor_PesoNegativo_LancaArgumentException()
    {
        Action act = () => CriarComponente(peso: -1);
        act.Should().Throw<ArgumentException>().WithMessage("*Peso*");
    }

    [Fact]
    public void Construtor_PesoAcimaDoLimite_LancaArgumentException()
    {
        Action act = () => CriarComponente(peso: 600);
        act.Should().Throw<ArgumentException>().WithMessage("*Peso*");
    }

    [Fact]
    public void Construtor_GarantiaNegativa_LancaArgumentException()
    {
        Action act = () => CriarComponente(garantiaDias: -1);
        act.Should().Throw<ArgumentException>().WithMessage("*Garantia*");
    }

    [Fact]
    public void SetUnidade_NormalizaParaMaiusculas()
    {
        var c = CriarComponenteValido();
        c.SetUnidade("kg");
        c.Unidade.Should().Be("KG");
    }

    [Fact]
    public void Desativar_TornaInativo()
    {
        var c = CriarComponenteValido();
        c.Desativar();
        c.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Ativar_AposDesativar_TornaAtivoNovamente()
    {
        var c = CriarComponenteValido();
        c.Desativar();
        c.Ativar();
        c.Ativo.Should().BeTrue();
    }

    [Fact]
    public void AdicionarEquivalencia_NoMesmoComponente_LancaInvalidOperationException()
    {
        var c = CriarComponenteValido();
        Action act = () => c.AdicionarEquivalencia(c, TipoEquivalencia.Similar);
        act.Should().Throw<InvalidOperationException>();
    }

    private static Componente CriarComponenteValido() => CriarComponente();

    private static Componente CriarComponente(
        string sku = "PFD-001",
        string nome = "Pastilha de freio dianteira",
        string descricao = "Pastilha de freio cerâmica",
        string marca = "Bosch",
        string partNumber = "PN-12345",
        string codigoOEM = "OEM-9876",
        string codigoBarras = "7891234567890",
        string ncm = "87083010",
        string cest = "0102000",
        string categoria = "Freios",
        string unidade = "UN",
        decimal peso = 0.500m,
        int garantiaDias = 180)
    {
        return new Componente(
            sku, nome, descricao, marca, partNumber, codigoOEM, codigoBarras,
            ncm, cest, categoria, unidade, peso, garantiaDias);
    }
}
