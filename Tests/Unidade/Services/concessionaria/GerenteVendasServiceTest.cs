using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Concessionaria.GerenteVendas;
using CarStoreManager.Application.Services.Concessionaria;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services.Concessionaria;

/// <summary>
/// Testes unitários do GerenteVendasService — espelham o padrão dos testes
/// de VendedorService (mesmo CRUD, mesma estrutura de DadosFuncionario).
/// </summary>
public class GerenteVendasServiceTests
{
    private readonly Mock<IGerenteVendasRepository> _repo = new();
    private readonly GerenteVendasService _service;

    public GerenteVendasServiceTests()
    {
        _service = new GerenteVendasService(_repo.Object);
    }

    private static GerenteVendas CriarValido() => new(
        "Maria Gerente", "maria@x.com", "11999990000", "Senha@123",
        NivelFuncionario.Pleno, DateTime.Now.AddDays(1));

    // ==================== GET ====================

    [Fact]
    public async Task GetByIdAsync_Existente_RetornaDTO()
    {
        var g = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(g.Id)).ReturnsAsync(g);

        var r = await _service.GetByIdAsync(g.Id);

        r.IsSuccess.Should().BeTrue();
        r.Value!.Nome.Should().Be("Maria Gerente");
        // AnosEmpresa = 0 (entidade só aceita contratação ≥ hoje, então não há tempo "passado")
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((GerenteVendas?)null);

        var r = await _service.GetByIdAsync(Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task GetAllAsync_RetornaListaDeListaDTO()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { CriarValido(), CriarValido() });

        var r = await _service.GetAllAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().HaveCount(2);
    }

    // ==================== ADD ====================

    [Fact]
    public async Task AddAsync_DTOValido_PersisteESalva()
    {
        var dto = new CriarGerenteVendasDTO
        {
            Nome = "Novo", Email = "n@x.com", Telefone = "11988887777",
            Senha = "Senha@123", Nivel = "Junior", DataContratacao = DateTime.Now.AddDays(1)
        };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().NotBeEmpty();
        _repo.Verify(x => x.AddAsync(It.IsAny<GerenteVendas>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NivelInvalido_RetornaFail()
    {
        var dto = new CriarGerenteVendasDTO
        {
            Nome = "x", Email = "x@x.com", Telefone = "11999990000",
            Senha = "Senha@123", Nivel = "Nivel-Inventado", DataContratacao = DateTime.Now.AddDays(1)
        };

        var r = await _service.AddAsync(dto);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("Nível");
        _repo.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    // ==================== UPDATE ====================

    [Fact]
    public async Task UpdateAsync_Existente_AtualizaCampos()
    {
        var g = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(g.Id)).ReturnsAsync(g);

        var r = await _service.UpdateAsync(new AtualizarGerenteVendasDTO
        {
            Id = g.Id, Email = "novo@x.com", Telefone = "11977776666", Nivel = "Senior"
        });

        r.IsSuccess.Should().BeTrue();
        g.GetEmail().Should().Be("novo@x.com");
        g.GetNivel().Should().Be("Senior");
        _repo.Verify(x => x.Update(g), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((GerenteVendas?)null);

        var r = await _service.UpdateAsync(new AtualizarGerenteVendasDTO { Id = Guid.NewGuid() });

        r.IsSuccess.Should().BeFalse();
        _repo.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    // ==================== REMOVE ====================

    [Fact]
    public async Task RemoveAsync_Existente_RemoveESalva()
    {
        var g = CriarValido();
        _repo.Setup(r => r.GetByIdAsync(g.Id)).ReturnsAsync(g);

        var r = await _service.RemoveAsync(g.Id);

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.Remove(g), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Inexistente_RetornaFail()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((GerenteVendas?)null);

        var r = await _service.RemoveAsync(Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
    }
}
