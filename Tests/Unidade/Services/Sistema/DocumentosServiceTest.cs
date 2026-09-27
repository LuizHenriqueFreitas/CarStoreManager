using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Services.Sistema;

public class DocumentosServiceTests
{
    private readonly Mock<ITermoEntregaRepository> _termoEntregaRepo = new();
    private readonly Mock<IPropostaVendaRepository> _propostaRepo = new();
    private readonly Mock<IVistoriaRepository> _vistoriaRepo = new();
    private readonly Mock<IVeiculoVendaRepository> _veiculoVendaRepo = new();
    private readonly Mock<IVeiculoConsignacaoRepository> _consignacaoRepo = new();
    private readonly Mock<ITermoTestDriveRepository> _termoTestDriveRepo = new();
    private readonly Mock<ITestDriveRepository> _testDriveRepo = new();
    private readonly Mock<IVistoriaOrdemServicoRepository> _vistoriaOSRepo = new();
    private readonly Mock<IOrdemServicoRepository> _ordemServicoRepo = new();
    private readonly Mock<IVeiculoClienteRepository> _veiculoClienteRepo = new();
    private readonly Mock<IClienteRepository> _clienteRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly DocumentosService _service;

    public DocumentosServiceTests()
    {
        _service = new DocumentosService(
            _termoEntregaRepo.Object, _propostaRepo.Object, _vistoriaRepo.Object,
            _veiculoVendaRepo.Object, _consignacaoRepo.Object,
            _termoTestDriveRepo.Object, _testDriveRepo.Object,
            _vistoriaOSRepo.Object, _ordemServicoRepo.Object, _veiculoClienteRepo.Object,
            _clienteRepo.Object, _usuarioRepo.Object);
    }

    // ==================== Termos de entrega ====================

    [Fact]
    public async Task ListarTermosEntregaAsync_VeiculoProprio_EnriqueceClienteVendedorPlacaEFotos()
    {
        var cliente = ClienteQualquer();
        var vendedor = VendedorQualquer();
        var veiculo = new VeiculoVenda("Onix", "LT", "Prata", "1.0", 2022, 15000, "ABC1234", "12345678900",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 60000, 55000, 2024, AcessoriosVeiculo.Nenhum);
        var proposta = new PropostaVenda(vendedor.Id, veiculo.Id, cliente.Id, 55000, 0);
        var termo = new TermoEntrega(proposta.Id, Guid.NewGuid(), "Texto do termo");
        var vistoria = new Vistoria(proposta.Id, Guid.NewGuid());
        vistoria.Registrar("Sem avarias.", true);

        _termoEntregaRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TermoEntrega> { termo });
        _propostaRepo.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);
        _clienteRepo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);
        _veiculoVendaRepo.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _vistoriaRepo.Setup(r => r.ObterPorPropostaAsync(proposta.Id)).ReturnsAsync(new List<Vistoria> { vistoria });

        var result = await _service.ListarTermosEntregaAsync();

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Single();
        dto.TipoDocumento.Should().Be("Termo de entrega");
        dto.ClienteNome.Should().Be(cliente.GetNome());
        dto.ResponsavelNome.Should().Be(vendedor.GetNome());
        dto.ResponsavelPapel.Should().Be("Vendedor");
        dto.Placa.Should().Be(veiculo.GetPlacaCarro());
        dto.EntidadeTipoFotos.Should().Be("Vistoria");
        dto.EntidadeIdFotos.Should().Be(vistoria.Id);
    }

    [Fact]
    public async Task ListarTermosEntregaAsync_VeiculoConsignado_ResolvePlacaPelaConsignacao()
    {
        var cliente = ClienteQualquer();
        var vendedor = VendedorQualquer();
        var consignado = ConsignacaoQualquer(cliente.Id, vendedor.Id);
        var proposta = new PropostaVenda(vendedor.Id, consignado.Id, cliente.Id, 40000, 0, "VeiculoConsignacao");
        var termo = new TermoEntrega(proposta.Id, Guid.NewGuid(), "Texto");

        _termoEntregaRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TermoEntrega> { termo });
        _propostaRepo.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);
        _clienteRepo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);
        _consignacaoRepo.Setup(r => r.GetByIdAsync(consignado.Id)).ReturnsAsync(consignado);
        _vistoriaRepo.Setup(r => r.ObterPorPropostaAsync(proposta.Id)).ReturnsAsync(new List<Vistoria>());

        var result = await _service.ListarTermosEntregaAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Single().Placa.Should().Be(consignado.GetPlacaCarro());
    }

    [Fact]
    public async Task ListarTermosEntregaAsync_PropostaInexistente_DevolveDtoSemEnriquecimento()
    {
        var termo = new TermoEntrega(Guid.NewGuid(), Guid.NewGuid(), "Texto");
        _termoEntregaRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TermoEntrega> { termo });
        _propostaRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PropostaVenda?)null);

        var result = await _service.ListarTermosEntregaAsync();

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Single();
        dto.ClienteNome.Should().BeNull();
        dto.EntidadeTipoFotos.Should().BeNull();
    }

    // ==================== Termos de test drive ====================

    [Fact]
    public async Task ListarTermosTestDriveAsync_VeiculoProprio_EnriqueceClienteVendedorPlaca()
    {
        var cliente = ClienteQualquer();
        var vendedor = VendedorQualquer();
        var veiculo = new VeiculoVenda("HB20", "S", "Branco", "1.6", 2021, 20000, "XYZ9876", "12345678900",
            TipoCambio.Automatico, TipoCombustivel.Flex, 70000, 65000, 2024, AcessoriosVeiculo.Nenhum);
        var testDrive = new TestDrive(veiculo.Id, cliente.Id, vendedor.Id, DateTime.UtcNow);
        var termo = new TermoTestDrive(testDrive.Id, vendedor.Id, "Texto do termo de test drive");

        _termoTestDriveRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TermoTestDrive> { termo });
        _testDriveRepo.Setup(r => r.GetByIdAsync(testDrive.Id)).ReturnsAsync(testDrive);
        _clienteRepo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);
        _veiculoVendaRepo.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);

        var result = await _service.ListarTermosTestDriveAsync();

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Single();
        dto.TipoDocumento.Should().Be("Termo de test drive");
        dto.ClienteNome.Should().Be(cliente.GetNome());
        dto.ResponsavelNome.Should().Be(vendedor.GetNome());
        dto.Placa.Should().Be(veiculo.GetPlacaCarro());
        dto.EntidadeTipoFotos.Should().BeNull("test drive não tem fotos hoje");
    }

    [Fact]
    public async Task ListarTermosTestDriveAsync_VeiculoConsignado_ResolvePlacaPelaConsignacao()
    {
        var cliente = ClienteQualquer();
        var vendedor = VendedorQualquer();
        var consignado = ConsignacaoQualquer(cliente.Id, vendedor.Id);
        var testDrive = new TestDrive(consignado.Id, cliente.Id, vendedor.Id, DateTime.UtcNow, veiculoEntidadeTipo: "VeiculoConsignacao");
        var termo = new TermoTestDrive(testDrive.Id, vendedor.Id, "Texto");

        _termoTestDriveRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TermoTestDrive> { termo });
        _testDriveRepo.Setup(r => r.GetByIdAsync(testDrive.Id)).ReturnsAsync(testDrive);
        _clienteRepo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);
        _consignacaoRepo.Setup(r => r.GetByIdAsync(consignado.Id)).ReturnsAsync(consignado);

        var result = await _service.ListarTermosTestDriveAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Single().Placa.Should().Be(consignado.GetPlacaCarro());
    }

    // ==================== Contratos de consignação ====================

    [Fact]
    public async Task ListarContratosConsignacaoAsync_ComTexto_EnriqueceEIncluiFotos()
    {
        var cliente = ClienteQualquer();
        var vendedor = VendedorQualquer();
        var consignado = ConsignacaoQualquer(cliente.Id, vendedor.Id);

        _consignacaoRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<VeiculoConsignacao> { consignado });
        _clienteRepo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _usuarioRepo.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);

        var result = await _service.ListarContratosConsignacaoAsync();

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Single();
        dto.TipoDocumento.Should().Be("Contrato de consignação");
        dto.ClienteNome.Should().Be(cliente.GetNome());
        dto.ResponsavelNome.Should().Be(vendedor.GetNome());
        dto.Placa.Should().Be(consignado.GetPlacaCarro());
        dto.EntidadeTipoFotos.Should().Be("VeiculoConsignacao");
        dto.EntidadeIdFotos.Should().Be(consignado.Id);
    }

    [Fact]
    public async Task ListarContratosConsignacaoAsync_SemTextoContrato_NaoAparece()
    {
        var cliente = ClienteQualquer();
        var vendedor = VendedorQualquer();
        var semContrato = new VeiculoConsignacao(
            "Gol", "1.6", "Preto", "1.6", 2019, 60000, "DEF5678", "12345678900",
            TipoCambio.Manual, TipoCombustivel.Flex, cliente.Id, vendedor.Id,
            ComissaoConsignacao.CriarPorcentagem(30000, 10), "");

        _consignacaoRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<VeiculoConsignacao> { semContrato });

        var result = await _service.ListarContratosConsignacaoAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ==================== Contratos de OS ====================

    [Fact]
    public async Task ListarContratosOSAsync_Concluida_EnriqueceClienteVeiculoRecepcionista()
    {
        var cliente = ClienteQualquer();
        var recepcionista = RecepcionistaQualquer();
        var veiculoCliente = new VeiculoCliente(cliente.Id, "Civic", "EXL", "Cinza", 2020, "QWE4321");
        var ordem = new OrdemServico(veiculoCliente.Id, Guid.NewGuid(), cliente.Id,
            TipoServico.Manutencao, "Revisão geral", DateTime.UtcNow.AddDays(5), 300m);
        var vistoria = new VistoriaOrdemServico(ordem.Id, recepcionista.Id);
        vistoria.Concluir("Veículo sem avarias.");

        _vistoriaOSRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<VistoriaOrdemServico> { vistoria });
        _usuarioRepo.Setup(r => r.GetByIdAsync(recepcionista.Id)).ReturnsAsync(recepcionista);
        _ordemServicoRepo.Setup(r => r.GetByIdAsync(ordem.Id)).ReturnsAsync(ordem);
        _clienteRepo.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
        _veiculoClienteRepo.Setup(r => r.GetByIdAsync(veiculoCliente.Id)).ReturnsAsync(veiculoCliente);

        var result = await _service.ListarContratosOSAsync();

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.Single();
        dto.TipoDocumento.Should().Be("Contrato de OS");
        dto.Status.Should().Be("Concluída");
        dto.ClienteNome.Should().Be(cliente.GetNome());
        dto.ResponsavelNome.Should().Be(recepcionista.GetNome());
        dto.ResponsavelPapel.Should().Be("Recepcionista");
        dto.Placa.Should().Be(veiculoCliente.Placa.GetPlaca());
        dto.EntidadeTipoFotos.Should().Be("VistoriaOrdemServico");
        dto.EntidadeIdFotos.Should().Be(vistoria.Id);
    }

    [Fact]
    public async Task ListarContratosOSAsync_EmAndamento_StatusReflexteNaoConcluida()
    {
        var recepcionista = RecepcionistaQualquer();
        var vistoria = new VistoriaOrdemServico(Guid.NewGuid(), recepcionista.Id);

        _vistoriaOSRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<VistoriaOrdemServico> { vistoria });
        _usuarioRepo.Setup(r => r.GetByIdAsync(recepcionista.Id)).ReturnsAsync(recepcionista);
        _ordemServicoRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((OrdemServico?)null);

        var result = await _service.ListarContratosOSAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Single().Status.Should().Be("Em andamento");
    }

    // ==================== Auxiliares ====================

    private static Cliente ClienteQualquer() => new(
        "Cliente Teste", "c@x.com", "11988887777", "39053344705",
        new Endereco("Rua", "1", null, "Centro", "Cidade", "SP", "01001000"));

    private static Vendedor VendedorQualquer() => new(
        "Vendedor Teste", "v@x.com", "11988887777", "Senha1", NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(5));

    private static Recepcionista RecepcionistaQualquer() => new(
        "Recepcionista Teste", "r@x.com", "11988887777", "Senha1", NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(5));

    private static VeiculoConsignacao ConsignacaoQualquer(Guid clienteId, Guid vendedorId) => new(
        "Corolla", "XEI", "Prata", "2.0", 2022, 10000, "CON1122", "12345678900",
        TipoCambio.Automatico, TipoCombustivel.Flex, clienteId, vendedorId,
        ComissaoConsignacao.CriarPorcentagem(80000, 8), "Texto do contrato de consignação.");
}
