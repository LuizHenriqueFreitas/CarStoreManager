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
    private readonly Mock<IFornecedorRepository> _fornecedorRepoMock = new();
    private readonly Mock<IComponenteEquivalenteRepository> _equivalenteRepoMock = new();
    private readonly Mock<IEstoqueRepository> _estoqueRepoMock = new();
    private readonly ComponenteService _service;

    public ComponenteServiceTests()
    {
        _configRepoMock.Setup(r => r.ObterAsync()).ReturnsAsync(new ConfiguracaoSistema(true));
        _fornecedorRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Fornecedor("Fornecedor Teste", "11122233000183"));
        _equivalenteRepoMock.Setup(r => r.ObterPorComponenteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<ComponenteEquivalente>());
        _service = new ComponenteService(
            _repoMock.Object, _configRepoMock.Object, _fornecedorRepoMock.Object,
            _equivalenteRepoMock.Object, _estoqueRepoMock.Object);
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
            GarantiaDias = 90,
            FornecedorId = Guid.NewGuid()
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
               0.5m, 180, Guid.NewGuid());

    private static Componente CriarComponente(string sku, string nome, string codigoOEM = "", bool ativo = true)
    {
        var c = new Componente(sku, nome, "Descrição", "Marca", $"PN-{sku}",
            codigoOEM, "", "87083010", "", "Freios", "UN", 0.5m, 180, Guid.NewGuid());
        if (!ativo) c.Desativar();
        return c;
    }

    private void ConfigurarEstoque(Guid componenteId, int quantidade)
    {
        var est = new EstoqueComponente(componenteId, 0);
        if (quantidade > 0) est.Adicionar(quantidade);
        _estoqueRepoMock.Setup(r => r.ObterPorComponenteAsync(componenteId)).ReturnsAsync(est);
    }

    // ==================== ObterEquivalentesAsync ====================

    [Fact]
    public async Task ObterEquivalentesAsync_ComponenteInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);

        var result = await _service.ObterEquivalentesAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ObterEquivalentesAsync_SoOEM_RetornaApenasMatchPorCodigoOEM()
    {
        var alvo = CriarComponente("A", "Pastilha", codigoOEM: "OEM-X");
        var comOEM = CriarComponente("B", "Pastilha Paralela", codigoOEM: "OEM-X");
        var semOEM = CriarComponente("C", "Outra Peça", codigoOEM: "OEM-Y");

        _repoMock.Setup(r => r.GetByIdAsync(alvo.Id)).ReturnsAsync(alvo);
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente> { alvo, comOEM, semOEM });
        ConfigurarEstoque(comOEM.Id, 5);

        var result = await _service.ObterEquivalentesAsync(alvo.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(s => s.Id == comOEM.Id && s.OrigemOEM && !s.OrigemCurada);
    }

    [Fact]
    public async Task ObterEquivalentesAsync_SoCurada_RetornaLigacaoMesmoSemCodigoOEM()
    {
        var alvo = CriarComponente("A", "Pastilha"); // sem OEM
        var curado = CriarComponente("B", "Pastilha Genérica");
        var ligacao = new ComponenteEquivalente(alvo.Id, curado.Id, TipoEquivalencia.Similar);
        typeof(ComponenteEquivalente).GetProperty("ComponenteEquivalenteRelacionado")!.SetValue(ligacao, curado);

        _repoMock.Setup(r => r.GetByIdAsync(alvo.Id)).ReturnsAsync(alvo);
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente> { alvo, curado });
        _equivalenteRepoMock.Setup(r => r.ObterPorComponenteAsync(alvo.Id))
            .ReturnsAsync(new List<ComponenteEquivalente> { ligacao });
        ConfigurarEstoque(curado.Id, 3);

        var result = await _service.ObterEquivalentesAsync(alvo.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(s => s.Id == curado.Id && s.OrigemCurada && !s.OrigemOEM
            && s.TipoEquivalencia == TipoEquivalencia.Similar);
    }

    [Fact]
    public async Task ObterEquivalentesAsync_MesmoComponenteNasDuasTrilhas_DedupComOrigemOEMECuradaTrue()
    {
        var alvo = CriarComponente("A", "Pastilha", codigoOEM: "OEM-X");
        var comAmbos = CriarComponente("B", "Pastilha Paralela", codigoOEM: "OEM-X");
        var ligacao = new ComponenteEquivalente(alvo.Id, comAmbos.Id, TipoEquivalencia.Paralela);
        typeof(ComponenteEquivalente).GetProperty("ComponenteEquivalenteRelacionado")!.SetValue(ligacao, comAmbos);

        _repoMock.Setup(r => r.GetByIdAsync(alvo.Id)).ReturnsAsync(alvo);
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente> { alvo, comAmbos });
        _equivalenteRepoMock.Setup(r => r.ObterPorComponenteAsync(alvo.Id))
            .ReturnsAsync(new List<ComponenteEquivalente> { ligacao });
        ConfigurarEstoque(comAmbos.Id, 1);

        var result = await _service.ObterEquivalentesAsync(alvo.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.Single().OrigemOEM.Should().BeTrue();
        result.Value!.Single().OrigemCurada.Should().BeTrue();
    }

    [Fact]
    public async Task ObterEquivalentesAsync_AlvoInativo_AindaRetornaSugestoes()
    {
        var alvo = CriarComponente("A", "Pastilha Descontinuada", codigoOEM: "OEM-X", ativo: false);
        var substituto = CriarComponente("B", "Pastilha Nova", codigoOEM: "OEM-X");

        _repoMock.Setup(r => r.GetByIdAsync(alvo.Id)).ReturnsAsync(alvo);
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente> { alvo, substituto });
        ConfigurarEstoque(substituto.Id, 2);

        var result = await _service.ObterEquivalentesAsync(alvo.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(s => s.Id == substituto.Id);
    }

    [Fact]
    public async Task ObterEquivalentesAsync_CandidatoCuradoInativo_EExcluido()
    {
        var alvo = CriarComponente("A", "Pastilha");
        var candidatoInativo = CriarComponente("B", "Pastilha Fora de Linha", ativo: false);
        var ligacao = new ComponenteEquivalente(alvo.Id, candidatoInativo.Id, TipoEquivalencia.Similar);
        typeof(ComponenteEquivalente).GetProperty("ComponenteEquivalenteRelacionado")!.SetValue(ligacao, candidatoInativo);

        _repoMock.Setup(r => r.GetByIdAsync(alvo.Id)).ReturnsAsync(alvo);
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente> { alvo, candidatoInativo });
        _equivalenteRepoMock.Setup(r => r.ObterPorComponenteAsync(alvo.Id))
            .ReturnsAsync(new List<ComponenteEquivalente> { ligacao });

        var result = await _service.ObterEquivalentesAsync(alvo.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterEquivalentesAsync_OrdenaEmEstoquePrimeiro()
    {
        var alvo = CriarComponente("A", "Pastilha", codigoOEM: "OEM-X");
        var semEstoque = CriarComponente("B", "Zebra Sem Estoque", codigoOEM: "OEM-X");
        var comEstoque = CriarComponente("C", "Alfa Com Estoque", codigoOEM: "OEM-X");

        _repoMock.Setup(r => r.GetByIdAsync(alvo.Id)).ReturnsAsync(alvo);
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Componente> { alvo, semEstoque, comEstoque });
        ConfigurarEstoque(semEstoque.Id, 0);
        ConfigurarEstoque(comEstoque.Id, 4);

        var result = await _service.ObterEquivalentesAsync(alvo.Id);

        result.Value!.First().Id.Should().Be(comEstoque.Id);
    }

    // ==================== ListarLigacoesEquivalenciaAsync ====================

    [Fact]
    public async Task ListarLigacoesEquivalenciaAsync_RetornaAmbasDirecoes()
    {
        var a = CriarComponente("A", "Peça A");
        var b = CriarComponente("B", "Peça B");
        var ligacao = new ComponenteEquivalente(a.Id, b.Id, TipoEquivalencia.Original);
        typeof(ComponenteEquivalente).GetProperty("ComponenteEquivalenteRelacionado")!.SetValue(ligacao, b);

        _repoMock.Setup(r => r.GetByIdAsync(a.Id)).ReturnsAsync(a);
        _equivalenteRepoMock.Setup(r => r.ObterPorComponenteAsync(a.Id))
            .ReturnsAsync(new List<ComponenteEquivalente> { ligacao });
        ConfigurarEstoque(b.Id, 1);

        var result = await _service.ListarLigacoesEquivalenciaAsync(a.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(l => l.ComponenteRelacionadoId == b.Id && l.TipoEquivalencia == TipoEquivalencia.Original);
    }

    // ==================== CriarLigacaoEquivalenciaAsync ====================

    [Fact]
    public async Task CriarLigacaoEquivalenciaAsync_MesmoComponente_RetornaFalha()
    {
        var id = Guid.NewGuid();
        var result = await _service.CriarLigacaoEquivalenciaAsync(new CriarComponenteEquivalenteDTO
        {
            ComponenteOriginalId = id,
            ComponenteEquivalenteId = id,
            TipoEquivalencia = TipoEquivalencia.Similar
        });

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CriarLigacaoEquivalenciaAsync_JaExisteEmQualquerDirecao_RetornaFalha()
    {
        var a = CriarComponente("A", "Peça A");
        var b = CriarComponente("B", "Peça B");
        _repoMock.Setup(r => r.GetByIdAsync(a.Id)).ReturnsAsync(a);
        _repoMock.Setup(r => r.GetByIdAsync(b.Id)).ReturnsAsync(b);
        _equivalenteRepoMock.Setup(r => r.ObterLigacaoEntreAsync(a.Id, b.Id))
            .ReturnsAsync(new ComponenteEquivalente(b.Id, a.Id, TipoEquivalencia.Similar));

        var result = await _service.CriarLigacaoEquivalenciaAsync(new CriarComponenteEquivalenteDTO
        {
            ComponenteOriginalId = a.Id,
            ComponenteEquivalenteId = b.Id,
            TipoEquivalencia = TipoEquivalencia.Similar
        });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("já está registrada");
    }

    [Fact]
    public async Task CriarLigacaoEquivalenciaAsync_Valido_CriaERetornaGuid()
    {
        var a = CriarComponente("A", "Peça A");
        var b = CriarComponente("B", "Peça B");
        _repoMock.Setup(r => r.GetByIdAsync(a.Id)).ReturnsAsync(a);
        _repoMock.Setup(r => r.GetByIdAsync(b.Id)).ReturnsAsync(b);
        _equivalenteRepoMock.Setup(r => r.ObterLigacaoEntreAsync(a.Id, b.Id)).ReturnsAsync((ComponenteEquivalente?)null);
        _equivalenteRepoMock.Setup(r => r.AddAsync(It.IsAny<ComponenteEquivalente>())).Returns(Task.CompletedTask);
        _equivalenteRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.CriarLigacaoEquivalenciaAsync(new CriarComponenteEquivalenteDTO
        {
            ComponenteOriginalId = a.Id,
            ComponenteEquivalenteId = b.Id,
            TipoEquivalencia = TipoEquivalencia.Paralela
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        _equivalenteRepoMock.Verify(r => r.AddAsync(It.IsAny<ComponenteEquivalente>()), Times.Once);
    }

    // ==================== RemoverLigacaoEquivalenciaAsync ====================

    [Fact]
    public async Task RemoverLigacaoEquivalenciaAsync_Existente_RemoveComSucesso()
    {
        var ligacao = new ComponenteEquivalente(Guid.NewGuid(), Guid.NewGuid(), TipoEquivalencia.Similar);
        _equivalenteRepoMock.Setup(r => r.GetByIdAsync(ligacao.Id)).ReturnsAsync(ligacao);
        _equivalenteRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RemoverLigacaoEquivalenciaAsync(ligacao.Id);

        result.IsSuccess.Should().BeTrue();
        _equivalenteRepoMock.Verify(r => r.Remove(ligacao), Times.Once);
    }

    [Fact]
    public async Task RemoverLigacaoEquivalenciaAsync_Inexistente_RetornaFalha()
    {
        _equivalenteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ComponenteEquivalente?)null);

        var result = await _service.RemoverLigacaoEquivalenciaAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
    }
}
