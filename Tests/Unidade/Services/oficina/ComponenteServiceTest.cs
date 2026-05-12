using Moq;
using FluentAssertions;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services;

public class ComponenteServiceTests
{
    private readonly Mock<IComponenteRepository> _repoMock = new();
    private readonly Mock<IConfiguracaoSistemaRepository> _configRepoMock = new();
    private readonly ComponenteService _service;

    public ComponenteServiceTests()
    {
        _configRepoMock.Setup(r => r.ObterAsync()).ReturnsAsync(new ConfiguracaoSistema(true));
        _service = new ComponenteService(_repoMock.Object, _configRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ComponenteExistente_RetornaDTO()
    {
        var componente = CriarComponenteValido();
        _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);

        var result = await _service.GetByIdAsync(componente.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Nome.Should().Be(componente.Nome);
        result.Value!.PartNumber.Should().Be("PN-12345");
    }

    [Fact]
    public async Task GetByIdAsync_ComponenteInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task GetAllAsync_ComDados_RetornaListaDTO()
    {
        var lista = new List<Componente> { CriarComponenteValido(), CriarComponenteValido(sku: "X-2") };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lista);

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count().Should().Be(2);
    }

    [Fact]
    public async Task AddAsync_DtoValido_CriaComponente()
    {
        var dto = new CriarComponenteDTO
        {
            SKUInterno = "SKU-1",
            Nome = "Filtro",
            Descricao = "Filtro de óleo",
            MarcaFabricante = "Bosch",
            PartNumber = "PN-FIL-1",
            CodigoOEM = "OEM-1",
            CodigoBarras = "7891234567890",
            NCM = "87083010",
            CEST = "0102000",
            Categoria = "Filtros",
            Unidade = "UN",
            Sistema = "Motor",
            Peso = 0.3m,
            GarantiaDias = 90
        };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Componente>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Componente>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NcmInvalido_RetornaFalha()
    {
        // Componente atualmente não valida campo Sistema (foi removido);
        // testamos validação real (NCM com formato errado).
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Componente>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CriarComponenteDTO
        {
            SKUInterno = "SKU-1",
            Nome = "Filtro",
            Descricao = "x",
            MarcaFabricante = "x",
            PartNumber = "PN-1",
            NCM = "ABC",
            Categoria = "x",
            Unidade = "UN",
            Peso = 0.1m,
            GarantiaDias = 1
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_ComponenteExistente_RemoveComSucesso()
    {
        var c = CriarComponenteValido();
        _repoMock.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(c);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RemoveAsync(c.Id);

        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.Remove(c), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ComponenteInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);

        var result = await _service.RemoveAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EntradaEstoqueAsync_AindaNaoImplementado_RetornaFail()
    {
        var result = await _service.EntradaEstoqueAsync(Guid.NewGuid(), 5);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SaidaEstoqueAsync_AindaNaoImplementado_RetornaFail()
    {
        var result = await _service.SaidaEstoqueAsync(Guid.NewGuid(), 5);
        result.IsSuccess.Should().BeFalse();
    }

    private static Componente CriarComponenteValido(string sku = "PFD-001")
        => new(sku, "Pastilha", "Pastilha de freio", "Bosch", "PN-12345",
               "OEM-1", "7891234567890", "87083010", "0102000", "Freios", "UN",
               0.5m, 180);
}
