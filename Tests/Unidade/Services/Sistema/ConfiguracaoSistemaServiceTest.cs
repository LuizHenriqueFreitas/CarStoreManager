using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Tests.Unidade.Services.Sistema;

/// <summary>
/// Testes unitários do ConfiguracaoSistemaService.
///
/// Cobre os pontos sensíveis que não são triviais:
///   • margens lidam com JSON malformado (fallback dictionary vazio),
///   • erros de validação da entidade viram Result.Fail (sem SaveChanges).
/// </summary>
public class ConfiguracaoSistemaServiceTests
{
    private readonly Mock<IConfiguracaoSistemaRepository> _repoMock = new();
    private readonly ConfiguracaoSistemaService _service;

    public ConfiguracaoSistemaServiceTests()
    {
        _service = new ConfiguracaoSistemaService(_repoMock.Object);
    }

    private ConfiguracaoSistema NovaCfg() => new(true);

    // ==================== ATUALIZAR ====================

    [Fact]
    public async Task AtualizarAsync_PersisteEntradaMinima()
    {
        var cfg = NovaCfg();
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            ExigirEntradaMinima = true, PercentualEntradaMinima = 20m
        });

        cfg.ExigirEntradaMinima.Should().BeTrue();
        cfg.PercentualEntradaMinima.Should().Be(20m);
    }

    [Fact]
    public async Task AtualizarAsync_DesabilitaEntradaMinima_ZeraPercentual()
    {
        var cfg = NovaCfg();
        cfg.ConfigurarEntradaMinima(true, 30m);
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            ExigirEntradaMinima = false, PercentualEntradaMinima = 99m // ignorado
        });

        cfg.ExigirEntradaMinima.Should().BeFalse();
        cfg.PercentualEntradaMinima.Should().Be(0m);
    }

    [Fact]
    public async Task AtualizarAsync_PercentualInvalido_RetornaFailSemSalvar()
    {
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(NovaCfg());

        var r = await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            ExigirEntradaMinima = true, PercentualEntradaMinima = 150m // inválido
        });

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    // ==================== MARGENS ====================

    [Fact]
    public async Task ObterMargensAsync_JsonValido_DeserializaCorretamente()
    {
        var cfg = NovaCfg();
        cfg.AtualizarMargens(new Dictionary<string, decimal>
        {
            ["Motor"] = 35m, ["Freios"] = 50m
        }, padraoGlobal: 25m);
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        var r = await _service.ObterMargensAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value!.MargensPorSistema.Should().ContainKey("Motor").WhoseValue.Should().Be(35m);
        r.Value.MargemPadraoGlobalPct.Should().Be(25m);
    }

    [Fact]
    public async Task ObterMargensAsync_JsonMalformado_FallbackVazio()
    {
        var cfg = NovaCfg();
        // Força um JSON inválido via reflexão — o service deve não quebrar
        typeof(ConfiguracaoSistema)
            .GetProperty("MargensPorSistemaJson")!
            .SetValue(cfg, "{nao-eh-json}");
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        var r = await _service.ObterMargensAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value!.MargensPorSistema.Should().BeEmpty();
    }

    [Fact]
    public async Task AtualizarMargensAsync_PadraoNegativo_RetornaFail()
    {
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(NovaCfg());

        var r = await _service.AtualizarMargensAsync(new MargensDTO
        {
            MargemPadraoGlobalPct = -1m
        });

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AtualizarMargensAsync_DadosValidos_Persiste()
    {
        var cfg = NovaCfg();
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        var r = await _service.AtualizarMargensAsync(new MargensDTO
        {
            MargemPadraoGlobalPct = 40m,
            MargensPorSistema = new Dictionary<string, decimal> { ["Motor"] = 50m }
        });

        r.IsSuccess.Should().BeTrue();
        cfg.MargemPadraoGlobalPct.Should().Be(40m);
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}
