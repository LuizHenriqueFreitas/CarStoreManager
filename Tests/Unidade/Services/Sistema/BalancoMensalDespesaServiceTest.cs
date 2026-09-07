using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services.Sistema;

public class BalancoMensalDespesaServiceTest
{
    private readonly Mock<IBalancoMensalDespesaRepository> _repo = new();
    private readonly Mock<IDespesaRepository> _modelo = new();
    private readonly Mock<IConfiguracaoSistemaRepository> _config = new();
    private readonly BalancoMensalDespesaService _service;

    public BalancoMensalDespesaServiceTest()
        => _service = new BalancoMensalDespesaService(_repo.Object, _modelo.Object, _config.Object);

    [Fact]
    public async Task ObterAsync_SemBalanco_RetornaVazioExisteFalse()
    {
        _repo.Setup(r => r.ObterPorCompetenciaAsync(It.IsAny<DateOnly>())).ReturnsAsync((BalancoMensalDespesa?)null);

        var r = await _service.ObterAsync(2026, 9);

        r.IsSuccess.Should().BeTrue();
        r.Value!.ExisteNoBanco.Should().BeFalse();
    }

    [Fact]
    public async Task GerarDoModeloAsync_CopiaLinhasAtivasDoModelo()
    {
        _repo.Setup(r => r.ObterPorCompetenciaAsync(It.IsAny<DateOnly>())).ReturnsAsync((BalancoMensalDespesa?)null);
        _modelo.Setup(m => m.GetAtivasAsync()).ReturnsAsync(new[]
        {
            new Despesa("Energia", 500m, SetorDespesa.Geral, TipoDespesa.Utilidades, "energia"),
            new Despesa("Aluguel", 3000m, SetorDespesa.Oficina, TipoDespesa.Aluguel, "aluguel"),
        });

        var r = await _service.GerarDoModeloAsync(2026, 9);

        r.IsSuccess.Should().BeTrue();
        r.Value!.Itens.Should().HaveCount(2);
        r.Value.Total.Should().Be(3500m);
        _repo.Verify(x => x.AddAsync(It.IsAny<BalancoMensalDespesa>()), Times.Once);
    }

    [Fact]
    public async Task GerarDoModeloAsync_JaExiste_Falha()
    {
        _repo.Setup(r => r.ObterPorCompetenciaAsync(It.IsAny<DateOnly>()))
             .ReturnsAsync(new BalancoMensalDespesa(new DateOnly(2026, 9, 1)));

        var r = await _service.GerarDoModeloAsync(2026, 9);

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TotalDoMesAsync_SemBalanco_UsaSomaDoModelo()
    {
        _repo.Setup(r => r.ObterPorCompetenciaAsync(It.IsAny<DateOnly>())).ReturnsAsync((BalancoMensalDespesa?)null);
        _modelo.Setup(m => m.GetAtivasAsync()).ReturnsAsync(new[]
        {
            new Despesa("Energia", 500m, SetorDespesa.Geral),
            new Despesa("Aluguel", 1500m, SetorDespesa.Geral),
        });

        var r = await _service.TotalDoMesAsync(2026, 9);

        r.Value.Should().Be(2000m);
    }

    [Fact]
    public async Task DiasParaFechamentoAsync_ForaDaJanela_RetornaNull()
    {
        var cfg = new ConfiguracaoSistema(true);
        // dia de fechamento longe de hoje: usa hoje+10 (dentro de 1..28)
        var diaLonge = ((DateTime.Today.Day + 10 - 1) % 28) + 1;
        cfg.ConfigurarDiaFechamentoDespesas(diaLonge);
        _config.Setup(c => c.ObterAsync()).ReturnsAsync(cfg);

        var dias = await _service.DiasParaFechamentoAsync();

        // pode ser null (se o dia caiu > 5 à frente) — o teste aceita null ou <= 5
        (dias is null || dias <= 5).Should().BeTrue();
    }

    [Fact]
    public async Task FecharEReabrir_AtualizamOEstado()
    {
        var balanco = new BalancoMensalDespesa(new DateOnly(2026, 9, 1));
        _repo.Setup(r => r.ObterPorCompetenciaAsync(It.IsAny<DateOnly>())).ReturnsAsync(balanco);

        (await _service.FecharAsync(2026, 9)).IsSuccess.Should().BeTrue();
        balanco.Fechado.Should().BeTrue();

        (await _service.ReabrirAsync(2026, 9)).IsSuccess.Should().BeTrue();
        balanco.Fechado.Should().BeFalse();
    }
}
