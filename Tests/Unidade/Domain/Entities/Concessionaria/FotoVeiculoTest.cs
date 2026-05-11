using FluentAssertions;
using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Concessionaria;

public class FotoTest
{
    [Fact]
    public void Construtor_CamposValidos_CriaInstancia()
    {
        var entidadeId = Guid.NewGuid();
        var foto = new Foto("VeiculoVenda", entidadeId, "/uploads/veiculovenda/x.jpg",
            "x.jpg", 1024, "image/jpeg", 0);

        foto.EntidadeTipo.Should().Be("VeiculoVenda");
        foto.EntidadeId.Should().Be(entidadeId);
        foto.Url.Should().Be("/uploads/veiculovenda/x.jpg");
        foto.NomeArquivo.Should().Be("x.jpg");
        foto.TamanhoBytes.Should().Be(1024);
        foto.ContentType.Should().Be("image/jpeg");
        foto.Ordem.Should().Be(0);
    }

    [Fact]
    public void Construtor_VeiculoVendaPreencheVeiculoVendaId()
    {
        var id = Guid.NewGuid();
        var foto = new Foto("VeiculoVenda", id, "/u/x.jpg", "x", 1, "image/png", 0);
        foto.VeiculoVendaId.Should().Be(id);
    }

    [Fact]
    public void Construtor_OutraEntidade_NaoPreencheVeiculoVendaId()
    {
        var id = Guid.NewGuid();
        var foto = new Foto("Componente", id, "/u/x.jpg", "x", 1, "image/png", 0);
        foto.VeiculoVendaId.Should().BeNull();
    }

    [Fact]
    public void Construtor_EntidadeTipoVazio_LancaArgumentException()
    {
        Action act = () => new Foto("", Guid.NewGuid(), "/u/x.jpg", "x", 1, "image/png", 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_EntidadeIdVazio_LancaArgumentException()
    {
        Action act = () => new Foto("VeiculoVenda", Guid.Empty, "/u/x.jpg", "x", 1, "image/png", 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_UrlVazia_LancaArgumentException()
    {
        Action act = () => new Foto("VeiculoVenda", Guid.NewGuid(), "", "x", 1, "image/png", 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstrutorConveniencia_VeiculoId_DefineEntidadeTipoVeiculoVenda()
    {
        var id = Guid.NewGuid();
        var foto = new Foto(id, "/u/x.jpg", 0);

        foto.EntidadeTipo.Should().Be("VeiculoVenda");
        foto.EntidadeId.Should().Be(id);
        foto.VeiculoVendaId.Should().Be(id);
    }

    [Fact]
    public void AtualizarOrdem_AtualizaOrdem()
    {
        var foto = new Foto(Guid.NewGuid(), "/u/x.jpg", 0);
        foto.AtualizarOrdem(5);
        foto.Ordem.Should().Be(5);
    }
}
