using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services.Concessionaria;

public class TestDriveServiceTest
{
    private readonly Mock<ITestDriveRepository> _repo = new();
    private readonly Mock<IVeiculoVendaRepository> _veic = new();
    private readonly Mock<IClienteRepository> _cli = new();
    private readonly Mock<IVendedorRepository> _vend = new();
    private readonly TestDriveService _service;

    public TestDriveServiceTest()
        => _service = new TestDriveService(_repo.Object, _veic.Object, _cli.Object, _vend.Object);

    private static VeiculoVenda VeiculoDisponivel() => new(
        "Honda", "Civic", "Preto", "2.0", 2023, 10000, "ABC1D23", "12345678900",
        TipoCambio.Automatico, TipoCombustivel.Flex, 90000m, 70000m, 2024, AcessoriosVeiculo.Nenhum);

    private CriarTestDriveDTO DtoValido() => new()
    {
        VeiculoVendaId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        VendedorId = Guid.NewGuid(),
        DataHora = DateTime.Now.AddDays(1)
    };

    [Fact]
    public async Task AgendarAsync_VeiculoInexistente_Falha()
    {
        _veic.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);

        var r = await _service.AgendarAsync(DtoValido());

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("Veículo");
    }

    [Fact]
    public async Task AgendarAsync_VeiculoVendido_Falha()
    {
        var v = VeiculoDisponivel();
        v.MarcarComoVendido();
        _veic.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(v);

        var r = await _service.AgendarAsync(DtoValido());

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("vendido");
    }

    [Fact]
    public async Task AgendarAsync_ClienteInexistente_Falha()
    {
        _veic.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(VeiculoDisponivel());
        _cli.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

        var r = await _service.AgendarAsync(DtoValido());

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("Cliente");
    }

    [Fact]
    public async Task AgendarAsync_TudoValido_PersisteERetornaId()
    {
        _veic.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(VeiculoDisponivel());
        _cli.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ClienteQualquer());
        _vend.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Vendedor(
            "Vend", "v@x.com", "11988887777", "Senha1", NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(5)));
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<TestDrive>());

        var r = await _service.AgendarAsync(DtoValido());

        r.IsSuccess.Should().BeTrue();
        _repo.Verify(x => x.AddAsync(It.IsAny<TestDrive>()), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarStatusAsync_StatusInvalido_Falha()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync(new TestDrive(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now));

        var r = await _service.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO
        {
            Id = Guid.NewGuid(),
            Status = "Voando"
        });

        r.IsSuccess.Should().BeFalse();
    }

    private static Cliente ClienteQualquer() => new(
        "Cliente Teste", "c@x.com", "11988887777", "39053344705",
        new Endereco("Rua", "1", null, "Centro", "Cidade", "SP", "01001000"));
}
