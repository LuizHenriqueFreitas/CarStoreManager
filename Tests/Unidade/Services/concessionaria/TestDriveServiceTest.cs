using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using CarStoreManager.Application.DTOs.Concessionaria.TestDrive;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Repositories;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services.Concessionaria;

public class TestDriveServiceTest
{
    private readonly Mock<ITestDriveRepository> _repo = new();
    private readonly Mock<IVeiculoVendaRepository> _veic = new();
    private readonly Mock<IVeiculoConsignacaoRepository> _consig = new();
    private readonly Mock<IClienteRepository> _cli = new();
    private readonly Mock<IVendedorRepository> _vend = new();
    private readonly Mock<ITermoTestDriveRepository> _termoRepo = new();
    private readonly Mock<IBalancoMensalDespesaService> _balanco = new();
    private readonly TestDriveService _service;

    public TestDriveServiceTest()
        => _service = new TestDriveService(_repo.Object, _veic.Object, _consig.Object, _cli.Object, _vend.Object, _termoRepo.Object, _balanco.Object);

    private static VeiculoVenda VeiculoDisponivel() => new(
        "Honda", "Civic", "Preto", "2.0", 2023, 10000, "ABC1D23", "12345678900",
        TipoCambio.Automatico, TipoCombustivel.Flex, 90000m, 70000m, 2024, AcessoriosVeiculo.Nenhum);

    private static VeiculoConsignacao ConsignacaoAtiva() => new(
        "Toyota", "Corolla", "Branco", "2.0", 2022, 20000, "XYZ9A87", "12345678900",
        TipoCambio.Automatico, TipoCombustivel.Flex, Guid.NewGuid(), Guid.NewGuid(),
        CarStoreManager.Domain.ValueObjects.ComissaoConsignacao.CriarFixo(100000m, 5000m), "Contrato padrão");

    private CriarTestDriveDTO DtoValido() => new()
    {
        VeiculoVendaId = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        VendedorId = Guid.NewGuid(),
        DataHora = DateTime.Now.AddDays(1),
        TextoTermo = "O cliente assume a responsabilidade pelo veículo durante o test drive."
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
        // O termo de responsabilidade nasce junto (Rascunho), atrelado ao test drive criado.
        _termoRepo.Verify(x => x.AddAsync(It.Is<TermoTestDrive>(t => t.TextoTermo != "")), Times.Once);
        _termoRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AgendarAsync_SemTextoDoTermo_Falha()
    {
        _veic.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(VeiculoDisponivel());
        _cli.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ClienteQualquer());
        _vend.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Vendedor(
            "Vend", "v@x.com", "11988887777", "Senha1", NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(5)));

        var dto = DtoValido();
        dto.TextoTermo = "  ";

        var r = await _service.AgendarAsync(dto);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("termo");
        _repo.Verify(x => x.AddAsync(It.IsAny<TestDrive>()), Times.Never);
    }

    [Fact]
    public async Task AgendarAsync_ConsignadoAtivo_Sucesso()
    {
        _consig.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ConsignacaoAtiva());
        _cli.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ClienteQualquer());
        _vend.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Vendedor(
            "Vend", "v@x.com", "11988887777", "Senha1", NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(5)));

        var dto = DtoValido();
        dto.VeiculoEntidadeTipo = "VeiculoConsignacao";

        var r = await _service.AgendarAsync(dto);

        r.IsSuccess.Should().BeTrue();
        _veic.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _repo.Verify(x => x.AddAsync(It.Is<TestDrive>(t => t.IsConsignado)), Times.Once);
    }

    [Fact]
    public async Task AgendarAsync_ConsignadoNaoEncontrado_Falha()
    {
        _consig.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoConsignacao?)null);

        var dto = DtoValido();
        dto.VeiculoEntidadeTipo = "VeiculoConsignacao";

        var r = await _service.AgendarAsync(dto);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("consignado");
    }

    [Fact]
    public async Task TrocarVendedorAsync_NovoVendedorInexistente_Falha()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync(new TestDrive(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now));
        _vend.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Vendedor?)null);

        var r = await _service.TrocarVendedorAsync(Guid.NewGuid(), Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
        _repo.Verify(x => x.Update(It.IsAny<TestDrive>()), Times.Never);
    }

    [Fact]
    public async Task TrocarVendedorAsync_Valido_AtualizaVendedor()
    {
        var td = new TestDrive(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(td);
        var novoVendedorId = Guid.NewGuid();
        _vend.Setup(r => r.GetByIdAsync(novoVendedorId)).ReturnsAsync(new Vendedor(
            "Novo Vend", "nv@x.com", "11988887777", "Senha1", NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(5)));

        var r = await _service.TrocarVendedorAsync(td.Id, novoVendedorId);

        r.IsSuccess.Should().BeTrue();
        td.VendedorId.Should().Be(novoVendedorId);
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

    [Fact]
    public async Task AtualizarStatusAsync_MarcandoRealizado_LancaDespesaDeCombustivel()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync(new TestDrive(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now));
        _veic.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(VeiculoDisponivel());
        _cli.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(ClienteQualquer());
        _balanco.Setup(b => b.GerarDoModeloAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(CarStoreManager.Application.Common.Result<BalancoMensalDespesaDTO>.Ok(new()));
        _balanco.Setup(b => b.SalvarItemAsync(It.IsAny<SalvarItemBalancoDTO>()))
            .ReturnsAsync(CarStoreManager.Application.Common.Result.Ok());

        var r = await _service.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO { Id = Guid.NewGuid(), Status = "Realizado" });

        r.IsSuccess.Should().BeTrue();
        _balanco.Verify(b => b.SalvarItemAsync(It.Is<SalvarItemBalancoDTO>(
            i => i.Categoria == "Combustível - Test Drive" && i.Valor == 50m && i.Setor == "Concessionaria")), Times.Once);
    }

    [Fact]
    public async Task AtualizarStatusAsync_Cancelando_NaoLancaDespesa()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
             .ReturnsAsync(new TestDrive(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now));

        var r = await _service.AtualizarStatusAsync(new AtualizarStatusTestDriveDTO { Id = Guid.NewGuid(), Status = "Cancelado" });

        r.IsSuccess.Should().BeTrue();
        _balanco.Verify(b => b.SalvarItemAsync(It.IsAny<SalvarItemBalancoDTO>()), Times.Never);
    }

    private static Cliente ClienteQualquer() => new(
        "Cliente Teste", "c@x.com", "11988887777", "39053344705",
        new Endereco("Rua", "1", null, "Centro", "Cidade", "SP", "01001000"));
}
