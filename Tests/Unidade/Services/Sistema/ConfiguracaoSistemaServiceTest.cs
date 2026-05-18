using FluentAssertions;
using Moq;

using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Tests.Unidade.Services.Sistema;

/// <summary>
/// Testes unitários do ConfiguracaoSistemaService.
///
/// Cobre os pontos sensíveis que não são triviais:
///   • a senha SMTP vazia no DTO mantém a senha antiga (não sobrescreve),
///   • testar envio delega para IEmailService (sem persistir nada),
///   • margens lidam com JSON malformado (fallback dictionary vazio),
///   • erros de validação da entidade viram Result.Fail (sem SaveChanges).
/// </summary>
public class ConfiguracaoSistemaServiceTests
{
    private readonly Mock<IConfiguracaoSistemaRepository> _repoMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly ConfiguracaoSistemaService _service;

    public ConfiguracaoSistemaServiceTests()
    {
        _service = new ConfiguracaoSistemaService(_repoMock.Object, _emailMock.Object);
    }

    private ConfiguracaoSistema NovaCfg() => new(true);

    // ==================== OBTER ====================

    [Fact]
    public async Task ObterAsync_RetornaDTOSemSenhaSMTP()
    {
        var cfg = NovaCfg();
        cfg.AtualizarSmtp("smtp.x.com", 587, "user@x.com", "senha-secreta", true, "from@x.com", "From");
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        var r = await _service.ObterAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value!.SmtpHost.Should().Be("smtp.x.com");
        // Regra crítica: GET nunca devolve a senha
        r.Value.SmtpSenha.Should().BeEmpty();
        r.Value.SmtpConfigurado.Should().BeTrue();
    }

    // ==================== ATUALIZAR ====================

    [Fact]
    public async Task AtualizarAsync_SenhaVaziaNoDTO_MantemSenhaAntiga()
    {
        var cfg = NovaCfg();
        cfg.AtualizarSmtp("h", 587, "u", "senhaOriginal", true, "from@x.com", "n");
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        var dto = new ConfiguracaoSistemaDTO
        {
            SmtpHost = "h2", SmtpPort = 465, SmtpUsuario = "u2", SmtpSenha = "", SmtpUsarSsl = true,
            EmailRemetente = "from2@x.com", NomeRemetente = "n2"
        };

        var r = await _service.AtualizarAsync(dto);

        r.IsSuccess.Should().BeTrue();
        cfg.SmtpSenha.Should().Be("senhaOriginal");
        cfg.SmtpHost.Should().Be("h2");
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_SenhaPreenchida_SobrescreveSenha()
    {
        var cfg = NovaCfg();
        cfg.AtualizarSmtp("h", 587, "u", "antiga", true, "from@x.com", "n");
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            SmtpHost = "h", SmtpPort = 587, SmtpUsuario = "u",
            SmtpSenha = "NOVA", SmtpUsarSsl = true,
            EmailRemetente = "from@x.com", NomeRemetente = "n"
        });

        cfg.SmtpSenha.Should().Be("NOVA");
    }

    [Fact]
    public async Task AtualizarAsync_EmailFinanciadoraInvalido_RetornaFailSemSalvar()
    {
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(NovaCfg());

        var r = await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            NomeFinanciadora = "Banco X", EmailFinanciadora = "isso-nao-eh-email",
            SmtpPort = 587, SmtpUsarSsl = true
        });

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_PortaInvalida_RetornaFail()
    {
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(NovaCfg());

        var r = await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            SmtpPort = 0, // inválida
            SmtpHost = "h", EmailRemetente = "x@x.com"
        });

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_PersisteEntradaMinima()
    {
        var cfg = NovaCfg();
        _repoMock.Setup(r => r.ObterAsync()).ReturnsAsync(cfg);

        await _service.AtualizarAsync(new ConfiguracaoSistemaDTO
        {
            SmtpPort = 587, SmtpUsarSsl = true,
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
            SmtpPort = 587, SmtpUsarSsl = true,
            ExigirEntradaMinima = false, PercentualEntradaMinima = 99m // ignorado
        });

        cfg.ExigirEntradaMinima.Should().BeFalse();
        cfg.PercentualEntradaMinima.Should().Be(0m);
    }

    // ==================== TESTAR ENVIO ====================

    [Fact]
    public async Task TestarEnvioAsync_DelegaParaEmailService()
    {
        _emailMock.Setup(e => e.EnviarAsync("alvo@x.com", It.IsAny<string>(), It.IsAny<string>(), true))
            .ReturnsAsync(Result.Ok());

        var r = await _service.TestarEnvioAsync("alvo@x.com");

        r.IsSuccess.Should().BeTrue();
        _emailMock.Verify(e => e.EnviarAsync("alvo@x.com",
            It.Is<string>(s => s.Contains("Teste")), It.IsAny<string>(), true), Times.Once);
    }

    [Fact]
    public async Task TestarEnvioAsync_EmailServiceFalha_PropagaErro()
    {
        _emailMock.Setup(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
            .ReturnsAsync(Result.Fail("Conexão recusada"));

        var r = await _service.TestarEnvioAsync("x@x.com");

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Be("Conexão recusada");
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
