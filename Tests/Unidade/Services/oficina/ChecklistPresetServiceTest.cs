using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;
using CarStoreManager.Application.Services.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;

namespace CarStoreManager.Tests.Unidade.Services.Oficina;

/// <summary>
/// Testes do ChecklistPresetService.
///
/// O preset é editado por inteiro num único POST/PUT (substituirItens
/// recebe a lista inteira). Aqui validamos:
///   • criação com itens + flag de ativação,
///   • substituição total de itens em update,
///   • toggling de Ativo conforme DTO,
///   • Lookup DTO retorna apenas presets ativos com contagem correta,
///   • ordens recalculadas pela posição da lista (1..N).
/// </summary>
public class ChecklistPresetServiceTests
{
    private readonly Mock<IChecklistPresetRepository> _repo = new();
    private readonly ChecklistPresetService _service;

    public ChecklistPresetServiceTests()
    {
        _service = new ChecklistPresetService(_repo.Object);
    }

    // ==================== ADD ====================

    [Fact]
    public async Task AddAsync_DTOValido_PersisteESalva()
    {
        var dto = new SalvarChecklistPresetDTO
        {
            Nome = "Padrão",
            Itens = new() { "Óleo", "Pneus", "Bateria" },
            Ativo = true
        };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.AddAsync(It.Is<ChecklistPreset>(
            p => p.Nome == "Padrão" && p.Itens.Count == 3 && p.Ativo)), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NomeVazio_RetornaFail()
    {
        var r = await _service.AddAsync(new SalvarChecklistPresetDTO { Nome = "" });

        r.IsSuccess.Should().BeFalse();
        _repo.Verify(x => x.AddAsync(It.IsAny<ChecklistPreset>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ItensNulo_CriaPresetSemItens()
    {
        // SubstituirItens lida com null por padrão (?? new())
        var dto = new SalvarChecklistPresetDTO { Nome = "Vazio", Itens = null! };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.AddAsync(It.Is<ChecklistPreset>(p => p.Itens.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task AddAsync_AtivoFalso_PersisteDesativado()
    {
        var dto = new SalvarChecklistPresetDTO { Nome = "Inativo", Ativo = false };

        await _service.AddAsync(dto);

        _repo.Verify(x => x.AddAsync(It.Is<ChecklistPreset>(p => !p.Ativo)), Times.Once);
    }

    // ==================== UPDATE ====================

    [Fact]
    public async Task UpdateAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ChecklistPreset?)null);

        var r = await _service.UpdateAsync(new SalvarChecklistPresetDTO { Id = Guid.NewGuid(), Nome = "x" });

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_SubstituiItensCompletamente()
    {
        var preset = new ChecklistPreset("Original");
        preset.AdicionarItem("antigo 1");
        preset.AdicionarItem("antigo 2");
        _repo.Setup(r => r.GetByIdAsync(preset.Id)).ReturnsAsync(preset);

        await _service.UpdateAsync(new SalvarChecklistPresetDTO
        {
            Id = preset.Id, Nome = "Original", Ativo = true,
            Itens = new() { "novo único" }
        });

        preset.Itens.Should().ContainSingle(i => i.Descricao == "novo único");
    }

    [Fact]
    public async Task UpdateAsync_DesativaQuandoFlagFalsa()
    {
        var preset = new ChecklistPreset("X");
        _repo.Setup(r => r.GetByIdAsync(preset.Id)).ReturnsAsync(preset);

        await _service.UpdateAsync(new SalvarChecklistPresetDTO
        {
            Id = preset.Id, Nome = "X", Ativo = false, Itens = new()
        });

        preset.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ReativaQuandoFlagVerdadeira()
    {
        var preset = new ChecklistPreset("X");
        preset.Desativar();
        _repo.Setup(r => r.GetByIdAsync(preset.Id)).ReturnsAsync(preset);

        await _service.UpdateAsync(new SalvarChecklistPresetDTO
        {
            Id = preset.Id, Nome = "X", Ativo = true, Itens = new()
        });

        preset.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NomeVazio_RetornaFailSemSalvar()
    {
        var preset = new ChecklistPreset("antigo");
        _repo.Setup(r => r.GetByIdAsync(preset.Id)).ReturnsAsync(preset);

        var r = await _service.UpdateAsync(new SalvarChecklistPresetDTO
        {
            Id = preset.Id, Nome = "", Ativo = true, Itens = new()
        });

        r.IsSuccess.Should().BeFalse();
        _repo.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    // ==================== REMOVE ====================

    [Fact]
    public async Task RemoveAsync_Existente_RemoveESalva()
    {
        var preset = new ChecklistPreset("X");
        _repo.Setup(r => r.GetByIdAsync(preset.Id)).ReturnsAsync(preset);

        var r = await _service.RemoveAsync(preset.Id);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.Remove(preset), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ChecklistPreset?)null);

        var r = await _service.RemoveAsync(Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
    }

    // ==================== LOOKUP / DTO ====================

    [Fact]
    public async Task GetLookupAtivosAsync_RetornaApenasAtivosComContagem()
    {
        var ativo = new ChecklistPreset("Ativo");
        ativo.AdicionarItem("a");
        ativo.AdicionarItem("b");

        _repo.Setup(r => r.GetAtivosAsync()).ReturnsAsync(new[] { ativo });

        var r = await _service.GetLookupAtivosAsync();

        r.IsSuccess.Should().BeTrue();
        var lookup = r.Value!.Single();
        lookup.Nome.Should().Be("Ativo");
        lookup.QuantidadeItens.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_RetornaItensOrdenadosPorOrdem()
    {
        var preset = new ChecklistPreset("Multi");
        preset.AdicionarItem("Primeiro");
        preset.AdicionarItem("Segundo");
        preset.AdicionarItem("Terceiro");
        _repo.Setup(r => r.GetByIdAsync(preset.Id)).ReturnsAsync(preset);

        var r = await _service.GetByIdAsync(preset.Id);

        r.IsSuccess.Should().BeTrue();
        r.Value!.Itens.Select(i => i.Descricao)
            .Should().ContainInOrder("Primeiro", "Segundo", "Terceiro");
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ChecklistPreset?)null);

        var r = await _service.GetByIdAsync(Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
    }
}
