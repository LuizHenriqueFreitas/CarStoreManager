using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Oficina.ChefeOficina;
using CarStoreManager.Application.Services.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;

namespace CarStoreManager.Tests.Unidade.Services.Oficina;

public class ChefeOficinaServiceTests
{
    private readonly Mock<IChefeOficinaRepository> _repo = new();
    private readonly ChefeOficinaService _service;

    public ChefeOficinaServiceTests()
    {
        _service = new ChefeOficinaService(_repo.Object);
    }

    private static ChefeOficina CriarValido() => new(
        "João Chefe", "joao@x.com", "11988889999", "Senha@123",
        NivelFuncionario.Senior, DateTime.Now.AddDays(1));

    [Fact]
    public async Task GetByIdAsync_Existente_RetornaDTO()
    {
        var c = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(c);

        var r = await _service.GetByIdAsync(c.Id);

        r.IsSuccess.Should().BeTrue();
        r.Value!.Nome.Should().Be("João Chefe");
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ChefeOficina?)null);
        var r = await _service.GetByIdAsync(Guid.NewGuid());
        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_RetornaLista()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { CriarValido() });
        var r = await _service.GetAllAsync();
        r.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task AddAsync_DTOValido_PersisteESalva()
    {
        _repo.Setup(x => x.AddAsync(It.IsAny<ChefeOficina>())).Returns(Task.CompletedTask);
        _repo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CriarChefeOficinaDTO
        {
            Nome = "Novo Chefe", Email = "n@x.com", Telefone = "11988887777",
            Senha = "Senha@123", Nivel = "Pleno", DataContratacao = DateTime.Now.AddDays(1)
        };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeTrue(r.Error);
        _repo.Verify(x => x.AddAsync(It.IsAny<ChefeOficina>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NivelInvalido_RetornaFail()
    {
        var dto = new CriarChefeOficinaDTO
        {
            Nome = "x", Email = "x@x.com", Telefone = "11988887777",
            Senha = "Senha@123", Nivel = "Master-Inexistente", DataContratacao = DateTime.Now.AddDays(1)
        };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Existente_AtualizaNivel()
    {
        var c = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(c);

        await _service.UpdateAsync(new AtualizarChefeOficinaDTO
        {
            Id = c.Id, Email = "n@x.com", Telefone = "11988887777", Nivel = "Pleno"
        });

        c.GetNivel().Should().Be("Pleno");
    }

    [Fact]
    public async Task RemoveAsync_Existente_RemoveESalva()
    {
        var c = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(c);

        var r = await _service.RemoveAsync(c.Id);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.Remove(c), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ChefeOficina?)null);
        var r = await _service.RemoveAsync(Guid.NewGuid());
        r.IsSuccess.Should().BeFalse();
    }
}
