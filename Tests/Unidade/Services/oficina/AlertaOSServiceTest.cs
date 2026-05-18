using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Services.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services.Oficina;

/// <summary>
/// Testes do AlertaOSService.
///
/// O alerta é o canal de comunicação mecânico → cliente quando aumenta o
/// escopo. O service orquestra a entidade Alerta + a OS (pausar / retomar)
/// e precisa que ambas estejam alinhadas para SaveChanges acontecer.
/// </summary>
public class AlertaOSServiceTests
{
    private readonly Mock<IAlertaOSRepository> _alertasRepo = new();
    private readonly Mock<IOrdemServicoRepository> _ordensRepo = new();
    private readonly AlertaOSService _service;

    public AlertaOSServiceTests()
    {
        _service = new AlertaOSService(_alertasRepo.Object, _ordensRepo.Object);
    }

    private OrdemServico CriarOrdemEmAndamento()
    {
        var os = new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.Manutencao, "X", DateTime.UtcNow.AddDays(1), 100m);
        os.Iniciar(); // Pendente → EmAndamento
        return os;
    }

    // ==================== EMITIR ====================

    [Fact]
    public async Task EmitirAsync_OSEmAndamento_PausaOSEPersisteAlerta()
    {
        var os = CriarOrdemEmAndamento();
        _ordensRepo.Setup(r => r.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var r = await _service.EmitirAsync(os.Id, Guid.NewGuid(),
            new CriarAlertaOSDTO { Descricao = "Descobri vazamento de óleo" });

        r.IsSuccess.Should().BeTrue();
        r.Value!.Status.Should().Be("Pendente");
        os.Status.Should().Be(StatusOrdemServico.Pausada);
        _alertasRepo.Verify(a => a.AddAsync(It.IsAny<AlertaOS>()), Times.Once);
        _ordensRepo.Verify(o => o.Update(os), Times.Once);
        _alertasRepo.Verify(a => a.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EmitirAsync_OSInexistente_RetornaFail()
    {
        _ordensRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var r = await _service.EmitirAsync(Guid.NewGuid(), Guid.NewGuid(),
            new CriarAlertaOSDTO { Descricao = "x" });

        r.IsSuccess.Should().BeFalse();
        _alertasRepo.Verify(a => a.AddAsync(It.IsAny<AlertaOS>()), Times.Never);
    }

    [Fact]
    public async Task EmitirAsync_OSPendente_FalhaPorqueNaoPodePausar()
    {
        // Regra: só OS EmAndamento pode ser pausada
        var os = new OrdemServico(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TipoServico.Diagnostico, "X", DateTime.UtcNow.AddDays(1), 50m);
        _ordensRepo.Setup(r => r.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var r = await _service.EmitirAsync(os.Id, Guid.NewGuid(),
            new CriarAlertaOSDTO { Descricao = "Algo apareceu" });

        r.IsSuccess.Should().BeFalse();
        os.Status.Should().Be(StatusOrdemServico.Pendente);
        _alertasRepo.Verify(a => a.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EmitirAsync_DescricaoVazia_RetornaFail()
    {
        var os = CriarOrdemEmAndamento();
        _ordensRepo.Setup(r => r.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var r = await _service.EmitirAsync(os.Id, Guid.NewGuid(),
            new CriarAlertaOSDTO { Descricao = "" });

        r.IsSuccess.Should().BeFalse();
    }

    // ==================== LISTAR ====================

    [Fact]
    public async Task ListarPendentesAsync_DelegaParaRepoEFiltraPorStatus()
    {
        _alertasRepo.Setup(r => r.ObterPorStatusAsync(StatusAlertaOS.Pendente))
            .ReturnsAsync(new[] { new AlertaOS(Guid.NewGuid(), Guid.NewGuid(), "x") });

        var r = await _service.ListarPendentesAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task ListarPorOrdemAsync_DelegaParaRepo()
    {
        var osId = Guid.NewGuid();
        _alertasRepo.Setup(r => r.ObterPorOrdemAsync(osId))
            .ReturnsAsync(new[]
            {
                new AlertaOS(osId, Guid.NewGuid(), "a"),
                new AlertaOS(osId, Guid.NewGuid(), "b")
            });

        var r = await _service.ListarPorOrdemAsync(osId);

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().HaveCount(2);
    }

    // ==================== RESOLVER ====================

    [Fact]
    public async Task ResolverAsync_ClienteAprova_RegistraEAReabreOS()
    {
        var os = CriarOrdemEmAndamento();
        os.Pausar();
        var alerta = new AlertaOS(os.Id, Guid.NewGuid(), "Avaria nova");

        _alertasRepo.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _ordensRepo.Setup(r => r.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var recepcionistaId = Guid.NewGuid();
        var r = await _service.ResolverAsync(alerta.Id, recepcionistaId,
            new ResolverAlertaDTO { Aprovou = true, ObservacaoCliente = "OK pode" });

        r.IsSuccess.Should().BeTrue();
        alerta.Status.Should().Be(StatusAlertaOS.ClienteAprovou);
        alerta.ObservacaoCliente.Should().Be("OK pode");
        alerta.ResolvidoPor.Should().Be(recepcionistaId);
        os.Status.Should().Be(StatusOrdemServico.EmAndamento);
    }

    [Fact]
    public async Task ResolverAsync_ClienteRecusa_RegistraComoRecusadoEReabreOS()
    {
        var os = CriarOrdemEmAndamento();
        os.Pausar();
        var alerta = new AlertaOS(os.Id, Guid.NewGuid(), "x");
        _alertasRepo.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _ordensRepo.Setup(r => r.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var r = await _service.ResolverAsync(alerta.Id, Guid.NewGuid(),
            new ResolverAlertaDTO { Aprovou = false });

        r.IsSuccess.Should().BeTrue();
        alerta.Status.Should().Be(StatusAlertaOS.ClienteRecusou);
        os.Status.Should().Be(StatusOrdemServico.EmAndamento);
    }

    [Fact]
    public async Task ResolverAsync_AlertaInexistente_RetornaFail()
    {
        _alertasRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AlertaOS?)null);

        var r = await _service.ResolverAsync(Guid.NewGuid(), Guid.NewGuid(),
            new ResolverAlertaDTO { Aprovou = true });

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResolverAsync_OSInexistente_RetornaFail()
    {
        var alerta = new AlertaOS(Guid.NewGuid(), Guid.NewGuid(), "x");
        _alertasRepo.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _ordensRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var r = await _service.ResolverAsync(alerta.Id, Guid.NewGuid(),
            new ResolverAlertaDTO { Aprovou = true });

        r.IsSuccess.Should().BeFalse();
        _alertasRepo.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ResolverAsync_AlertaJaResolvido_RetornaFail()
    {
        var os = CriarOrdemEmAndamento();
        os.Pausar();
        var alerta = new AlertaOS(os.Id, Guid.NewGuid(), "x");
        alerta.RegistrarDecisaoCliente(true, Guid.NewGuid()); // já está resolvido

        _alertasRepo.Setup(r => r.GetByIdAsync(alerta.Id)).ReturnsAsync(alerta);
        _ordensRepo.Setup(r => r.GetByIdAsync(os.Id)).ReturnsAsync(os);

        var r = await _service.ResolverAsync(alerta.Id, Guid.NewGuid(),
            new ResolverAlertaDTO { Aprovou = false });

        r.IsSuccess.Should().BeFalse();
        _alertasRepo.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}
