// Este arquivo nao foi revisado nem documentado
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;
public class FotoVeiculoTest
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_CriaInstancia()
    {
        var veiculoId = Guid.NewGuid();
        var foto = new FotoVeiculo(veiculoId, "https://imagem.com/foto.jpg", 0);

        foto.VeiculoVendaId.Should().Be(veiculoId);
        foto.Url.Should().Be("https://imagem.com/foto.jpg");
        foto.Ordem.Should().Be(0);
    }

    [Fact]
    public void Construtor_VeiculoIdVazio_LancaArgumentException()
    {
        Action act = () => new FotoVeiculo(Guid.Empty, "url", 1);
        act.Should().Throw<ArgumentException>().WithMessage("*Veículo inválido*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_UrlInvalida_LancaArgumentException(string url)
    {
        Action act = () => new FotoVeiculo(Guid.NewGuid(), url, 1);
        act.Should().Throw<ArgumentException>().WithMessage("*URL da foto inválida*");
    }

    [Fact]
    public void Construtor_OrdemNegativa_LancaArgumentException()
    {
        Action act = () => new FotoVeiculo(Guid.NewGuid(), "url", -1);
        act.Should().Throw<ArgumentException>().WithMessage("*Ordem inválida*");
    }

    [Fact]
    public void Construtor_UrlComEspacos_RemoveEspacos()
    {
        var foto = new FotoVeiculo(Guid.NewGuid(), "  https://site.com/img.jpg  ", 2);
        foto.Url.Should().Be("https://site.com/img.jpg");
    }

    // ==================== ALTERAR ORDEM ====================

    [Fact]
    public void AlterarOrdem_ValorValido_AtualizaOrdem()
    {
        var foto = new FotoVeiculo(Guid.NewGuid(), "url", 1);
        foto.AlterarOrdem(3);
        foto.Ordem.Should().Be(3);
    }

    [Fact]
    public void AlterarOrdem_Negativa_LancaArgumentException()
    {
        var foto = new FotoVeiculo(Guid.NewGuid(), "url", 1);
        Action act = () => foto.AlterarOrdem(-1);
        act.Should().Throw<ArgumentException>().WithMessage("*Ordem inválida*");
    }

    [Fact]
    public void AlterarOrdem_Zero_DeveAceitar()
    {
        var foto = new FotoVeiculo(Guid.NewGuid(), "url", 5);
        foto.AlterarOrdem(0);
        foto.Ordem.Should().Be(0);
    }
}