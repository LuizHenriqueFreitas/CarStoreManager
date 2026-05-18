using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Recepcionista;
using CarStoreManager.Application.Services.Shared;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services;

public class RecepcionistaServiceTests
{
    private readonly Mock<IRecepcionistaRepository> _repo = new();
    private readonly RecepcionistaService _service;

    public RecepcionistaServiceTests()
    {
        _service = new RecepcionistaService(_repo.Object);
    }

    private static Recepcionista CriarValido() => new(
        "Ana Recepcionista", "ana@x.com", "11977776666", "Senha@123",
        NivelFuncionario.Junior, DateTime.Now.AddDays(1));

    [Fact]
    public async Task GetByIdAsync_Existente_RetornaDTO()
    {
        var rec = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(rec.Id)).ReturnsAsync(rec);

        var r = await _service.GetByIdAsync(rec.Id);

        r.IsSuccess.Should().BeTrue();
        r.Value!.Nome.Should().Be("Ana Recepcionista");
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Recepcionista?)null);
        var r = await _service.GetByIdAsync(Guid.NewGuid());
        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_RetornaLista()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { CriarValido(), CriarValido(), CriarValido() });
        var r = await _service.GetAllAsync();
        r.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddAsync_DTOValido_PersisteESalva()
    {
        var dto = new CriarRecepcionistaDTO
        {
            Nome = "Nova Rec.", Email = "nova@x.com", Telefone = "11988887777",
            Senha = "Senha@123", Nivel = "Junior", DataContratacao = DateTime.Now.AddDays(1)
        };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.AddAsync(It.IsAny<Recepcionista>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NivelInvalido_RetornaFail()
    {
        var r = await _service.AddAsync(new CriarRecepcionistaDTO
        {
            Nome = "x", Email = "x@x.com", Telefone = "11988887777",
            Senha = "Senha@123", Nivel = "XPTO", DataContratacao = DateTime.Now.AddDays(1)
        });

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Existente_AtualizaEmail()
    {
        var rec = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(rec.Id)).ReturnsAsync(rec);

        await _service.UpdateAsync(new AtualizarRecepcionistaDTO
        {
            Id = rec.Id, Email = "novo@x.com", Telefone = "11988887777", Nivel = "Pleno"
        });

        rec.GetEmail().Should().Be("novo@x.com");
        rec.GetNivel().Should().Be("Pleno");
    }

    [Fact]
    public async Task RemoveAsync_Existente_RemoveESalva()
    {
        var rec = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(rec.Id)).ReturnsAsync(rec);

        var r = await _service.RemoveAsync(rec.Id);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.Remove(rec), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Recepcionista?)null);
        var r = await _service.RemoveAsync(Guid.NewGuid());
        r.IsSuccess.Should().BeFalse();
    }
}
