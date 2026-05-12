using CarStoreManager.Application.Services.Oficina.NotaFiscal;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace CarStoreManager.Tests.Unidade.Services.NotaFiscal;

public class NotaFiscalEntradaServiceTest
{
    private const string XmlValido = """
        <?xml version="1.0" encoding="UTF-8"?>
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00">
          <NFe>
            <infNFe Id="NFe35260411222333000181550010000001231234567890" versao="4.00">
              <ide><serie>1</serie><nNF>123</nNF><dhEmi>2026-05-10T10:00:00-03:00</dhEmi></ide>
              <emit><CNPJ>11222333000181</CNPJ><xNome>Auto Pecas BR</xNome><xFant>APB</xFant></emit>
              <det nItem="1">
                <prod><cProd>PB-1234</cProd><xProd>Pastilha</xProd><NCM>87083010</NCM><CFOP>5102</CFOP>
                  <uCom>UN</uCom><qCom>10.0000</qCom><vUnCom>85.5000</vUnCom></prod>
                <imposto><ICMS><ICMS00><CST>00</CST><pICMS>18</pICMS><vICMS>153.90</vICMS></ICMS00></ICMS></imposto>
              </det>
              <total><ICMSTot><vProd>855.00</vProd><vICMS>153.90</vICMS><vIPI>0</vIPI><vPIS>0</vPIS><vCOFINS>0</vCOFINS><vNF>855.00</vNF></ICMSTot></total>
            </infNFe>
          </NFe>
        </nfeProc>
        """;

    private readonly Mock<INotaFiscalRepository> _notasRepo = new();
    private readonly Mock<IFornecedorRepository> _fornecedoresRepo = new();
    private readonly Mock<IComponenteRepository> _componentesRepo = new();
    private readonly Mock<IEstoqueRepository> _estoqueRepo = new();
    private readonly Mock<ILoteComponenteRepository> _lotesRepo = new();
    private readonly Mock<IOrdemServicoRepository> _ordensRepo = new();
    private readonly Mock<CarStoreManager.Domain.Interfaces.Repositories.Sistema.IConfiguracaoSistemaRepository> _configRepo = new();
    private readonly NotaFiscalEntradaService _service;

    public NotaFiscalEntradaServiceTest()
    {
        _configRepo.Setup(r => r.ObterAsync()).ReturnsAsync(new CarStoreManager.Domain.Entities.Sistema.ConfiguracaoSistema(true));
        _service = new NotaFiscalEntradaService(
            _notasRepo.Object,
            _fornecedoresRepo.Object,
            _componentesRepo.Object,
            _estoqueRepo.Object,
            _lotesRepo.Object,
            _ordensRepo.Object,
            _configRepo.Object);

        _componentesRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente>());
        _ordensRepo.Setup(r => r.ObterComItensAguardandoAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<CarStoreManager.Domain.Entities.Oficina.OrdemServico>());
    }

    [Fact]
    public async Task ImportarXmlAsync_NotaInexistente_CriaFornecedorENota()
    {
        _notasRepo.Setup(r => r.ObterPorChaveAsync(It.IsAny<string>()))
            .ReturnsAsync((CarStoreManager.Domain.Entities.Oficina.NotaFiscal?)null);
        _fornecedoresRepo.Setup(r => r.ObterPorCnpjAsync("11222333000181"))
            .ReturnsAsync((Fornecedor?)null);

        CarStoreManager.Domain.Entities.Oficina.NotaFiscal? capturada = null;
        _notasRepo.Setup(r => r.AddAsync(It.IsAny<CarStoreManager.Domain.Entities.Oficina.NotaFiscal>()))
            .Callback<CarStoreManager.Domain.Entities.Oficina.NotaFiscal>(n => capturada = n)
            .Returns(Task.CompletedTask);
        _notasRepo.Setup(r => r.ObterCompletoAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => capturada);

        var r = await _service.ImportarXmlAsync(XmlValido);

        r.IsSuccess.Should().BeTrue();
        capturada.Should().NotBeNull();
        capturada!.Numero.Should().Be("123");
        capturada.ChaveAcesso.Should().Be("35260411222333000181550010000001231234567890");
        capturada.Itens.Should().HaveCount(1);
        capturada.Status.Should().Be(StatusNotaFiscal.ImportadaAguardandoAprovacao);
        _fornecedoresRepo.Verify(r => r.AddAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task ImportarXmlAsync_ChaveDuplicada_RetornaFalha()
    {
        var existente = CriarNotaPendente();
        _notasRepo.Setup(r => r.ObterPorChaveAsync(It.IsAny<string>())).ReturnsAsync(existente);

        var r = await _service.ImportarXmlAsync(XmlValido);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("já importada");
    }

    [Fact]
    public async Task AprovarAsync_TodosItensMapeados_GeraLoteEEntradaEstoque()
    {
        var componente = CriarComponente();
        var nota = CriarNotaPendente(qty: 10, componente: componente);

        _notasRepo.Setup(r => r.ObterCompletoAsync(nota.Id)).ReturnsAsync(nota);
        _estoqueRepo.Setup(r => r.ObterPorComponenteAsync(componente.Id))
            .ReturnsAsync((EstoqueComponente?)null);

        var r = await _service.AprovarAsync(nota.Id);

        r.IsSuccess.Should().BeTrue();
        nota.Status.Should().Be(StatusNotaFiscal.Aprovada);
        nota.DataAprovacao.Should().NotBeNull();

        _lotesRepo.Verify(l => l.AddAsync(It.Is<LoteComponente>(
            x => x.ComponenteId == componente.Id && x.QuantidadeRecebida == 10)), Times.Once);

        _estoqueRepo.Verify(e => e.AddAsync(It.Is<EstoqueComponente>(
            x => x.PecaId == componente.Id && x.QuantidadeAtual == 10)), Times.Once);
    }

    [Fact]
    public async Task AprovarAsync_ItensNaoMapeados_Falha()
    {
        var nota = CriarNotaPendente(qty: 5, componente: null);
        _notasRepo.Setup(r => r.ObterCompletoAsync(nota.Id)).ReturnsAsync(nota);

        var r = await _service.AprovarAsync(nota.Id);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("vinculados");
        nota.Status.Should().Be(StatusNotaFiscal.ImportadaAguardandoAprovacao);
    }

    [Fact]
    public async Task RejeitarAsync_ComMotivo_AlteraStatus()
    {
        var nota = CriarNotaPendente();
        _notasRepo.Setup(r => r.GetByIdAsync(nota.Id)).ReturnsAsync(nota);

        var r = await _service.RejeitarAsync(nota.Id, "duplicada");

        r.IsSuccess.Should().BeTrue();
        nota.Status.Should().Be(StatusNotaFiscal.Rejeitada);
        nota.MotivoRejeicao.Should().Be("duplicada");
    }

    // helpers

    private static CarStoreManager.Domain.Entities.Oficina.NotaFiscal CriarNotaPendente(
        int qty = 10, Componente? componente = null)
    {
        var fornecedorId = Guid.NewGuid();
        var nota = new CarStoreManager.Domain.Entities.Oficina.NotaFiscal(
            tipo: TipoNotaFiscal.Entrada,
            numero: "123",
            serie: "1",
            chaveAcesso: "35260411222333000181550010000001231234567890",
            dataEmissao: new DateTime(2026, 5, 10),
            xmlConteudo: "<x/>",
            valorProdutos: 855m, valorImpostos: 153.90m, valorTotal: 855m,
            fornecedorId: fornecedorId);

        var item = new ItemNotaFiscal(
            notaFiscalId: nota.Id,
            codigoProdutoFornecedor: "PB-1234",
            descricaoProdutoFornecedor: "Pastilha",
            ncm: "87083010", unidade: "UN",
            quantidade: qty, valorUnitario: 85.50m,
            cfop: "5102", cst: "00", csosn: null,
            aliquotaIcms: 18m, valorIcms: 153.90m);

        if (componente is not null)
            item.VincularComponente(componente.Id);

        nota.AdicionarItem(item);
        return nota;
    }

    private static Componente CriarComponente()
    {
        var c = new Componente(
            "FIL-001", "Filtro", "Filtro de óleo", "Bosch", "PB-1234",
            "OEM-1", "7891234567890", "87083010", "0102000",
            "Motor", "UN", 0.3m, 90);
        typeof(Componente).BaseType?.GetProperty("Id")?.SetValue(c, Guid.NewGuid());
        return c;
    }
}
