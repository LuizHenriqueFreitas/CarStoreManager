using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Tests.Unidade.Domain.Entidades.Oficina;

public class VistoriaOrdemServicoTests
{
    // ==================== CONSTRUTOR ====================

    [Fact]
    public void Construtor_CamposValidos_CriaVistoriaEmAndamento()
    {
        var ordemServicoId = Guid.NewGuid();
        var recepcionistaId = Guid.NewGuid();

        var vistoria = new VistoriaOrdemServico(ordemServicoId, recepcionistaId);

        vistoria.OrdemServicoId.Should().Be(ordemServicoId);
        vistoria.RecepcionistaId.Should().Be(recepcionistaId);
        vistoria.Concluida.Should().BeFalse();
        vistoria.DataConclusao.Should().BeNull();
        vistoria.TextoContrato.Should().BeEmpty();
    }

    [Fact]
    public void Construtor_OrdemServicoIdVazio_LancaArgumentException()
    {
        Action act = () => new VistoriaOrdemServico(Guid.Empty, Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Construtor_RecepcionistaIdVazio_LancaArgumentException()
    {
        Action act = () => new VistoriaOrdemServico(Guid.NewGuid(), Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    // ==================== EditarTexto ====================

    [Fact]
    public void EditarTexto_VistoriaEmAndamento_AtualizaTexto()
    {
        var vistoria = new VistoriaOrdemServico(Guid.NewGuid(), Guid.NewGuid());

        vistoria.EditarTexto("Veículo com arranhão na porta dianteira esquerda.");

        vistoria.TextoContrato.Should().Be("Veículo com arranhão na porta dianteira esquerda.");
    }

    [Fact]
    public void EditarTexto_VistoriaConcluida_LancaInvalidOperationException()
    {
        var vistoria = new VistoriaOrdemServico(Guid.NewGuid(), Guid.NewGuid());
        vistoria.Concluir("Texto final do contrato.");

        Action act = () => vistoria.EditarTexto("Nova tentativa de edição.");

        act.Should().Throw<InvalidOperationException>();
    }

    // ==================== Concluir ====================

    [Fact]
    public void Concluir_TextoValido_MarcaConcluidaEDataConclusao()
    {
        var vistoria = new VistoriaOrdemServico(Guid.NewGuid(), Guid.NewGuid());

        vistoria.Concluir("Estado do veículo registrado, sem avarias.");

        vistoria.Concluida.Should().BeTrue();
        vistoria.DataConclusao.Should().NotBeNull();
        vistoria.TextoContrato.Should().Be("Estado do veículo registrado, sem avarias.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Concluir_TextoVazio_LancaArgumentException(string? texto)
    {
        var vistoria = new VistoriaOrdemServico(Guid.NewGuid(), Guid.NewGuid());

        Action act = () => vistoria.Concluir(texto!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Concluir_VistoriaJaConcluida_LancaInvalidOperationException()
    {
        var vistoria = new VistoriaOrdemServico(Guid.NewGuid(), Guid.NewGuid());
        vistoria.Concluir("Primeira conclusão.");

        Action act = () => vistoria.Concluir("Segunda tentativa.");

        act.Should().Throw<InvalidOperationException>();
    }
}
