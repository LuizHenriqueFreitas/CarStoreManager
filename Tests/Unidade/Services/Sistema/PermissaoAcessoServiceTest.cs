using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Tests.Unidade.Services.Sistema;

public class PermissaoAcessoServiceTests
{
    private const string ChaveFornecedores = "pagina:/oficina/fornecedores"; // Admin, Mecanico, ChefeOficina
    private const string ChaveNovoFornecedor = "acao:oficina.novo-fornecedor"; // Admin

    private readonly Mock<IPermissaoAcessoRepository> _repoPapel = new();
    private readonly Mock<IPermissaoIndividualRepository> _repoIndividual = new();
    private readonly PermissaoAcessoService _service;

    public PermissaoAcessoServiceTests()
    {
        _repoPapel.Setup(r => r.GetAsync(It.IsAny<RoleUsuario>(), It.IsAny<string>())).ReturnsAsync((PermissaoAcesso?)null);
        _repoIndividual.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync((PermissaoIndividual?)null);
        _service = new PermissaoAcessoService(_repoPapel.Object, _repoIndividual.Object);
    }

    // ==================== PodeAcessarAsync(usuarioId, role, chave) ====================

    [Fact]
    public async Task PodeAcessarAsync_SemOverrideIndividual_CaiNoComportamentoDoPapel()
    {
        var usuarioId = Guid.NewGuid();

        var resultado = await _service.PodeAcessarAsync(usuarioId, RoleUsuario.Mecanico, ChaveFornecedores);

        resultado.Should().BeTrue(); // Mecanico está no teto de ChaveFornecedores, sem override de papel = true
    }

    [Fact]
    public async Task PodeAcessarAsync_SemOverrideIndividual_PapelForaDoTeto_ContinuaFalso()
    {
        var usuarioId = Guid.NewGuid();

        var resultado = await _service.PodeAcessarAsync(usuarioId, RoleUsuario.Vendedor, ChaveFornecedores);

        resultado.Should().BeFalse(); // Vendedor não está no teto — comportamento de hoje intacto
    }

    [Fact]
    public async Task PodeAcessarAsync_OverrideIndividualTrue_AmpliaAlemDoPapel()
    {
        var usuarioId = Guid.NewGuid();
        _repoIndividual.Setup(r => r.GetAsync(usuarioId, ChaveFornecedores))
            .ReturnsAsync(new PermissaoIndividual(usuarioId, ChaveFornecedores, true));

        // Vendedor normalmente NÃO acessa /oficina/fornecedores
        var resultado = await _service.PodeAcessarAsync(usuarioId, RoleUsuario.Vendedor, ChaveFornecedores);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task PodeAcessarAsync_OverrideIndividualFalse_RestringeAlemDoPapel()
    {
        var usuarioId = Guid.NewGuid();
        _repoIndividual.Setup(r => r.GetAsync(usuarioId, ChaveFornecedores))
            .ReturnsAsync(new PermissaoIndividual(usuarioId, ChaveFornecedores, false));

        // Mecanico normalmente acessa /oficina/fornecedores
        var resultado = await _service.PodeAcessarAsync(usuarioId, RoleUsuario.Mecanico, ChaveFornecedores);

        resultado.Should().BeFalse();
    }

    // ==================== ObterMatrizIndividualAsync ====================

    [Fact]
    public async Task ObterMatrizIndividualAsync_RetornaPadraoDoPapelSemOverride()
    {
        var usuarioId = Guid.NewGuid();
        _repoIndividual.Setup(r => r.GetAllPorUsuarioAsync(usuarioId)).ReturnsAsync(new List<PermissaoIndividual>());

        var matriz = await _service.ObterMatrizIndividualAsync(usuarioId, RoleUsuario.Vendedor);

        var fornecedores = matriz.Single(m => m.Chave == ChaveFornecedores);
        fornecedores.PadraoDoPapel.Should().BeFalse();
        fornecedores.OverrideIndividual.Should().BeNull();
    }

    [Fact]
    public async Task ObterMatrizIndividualAsync_ComOverride_RetornaOValorSalvo()
    {
        var usuarioId = Guid.NewGuid();
        _repoIndividual.Setup(r => r.GetAllPorUsuarioAsync(usuarioId))
            .ReturnsAsync(new List<PermissaoIndividual> { new(usuarioId, ChaveFornecedores, true) });

        var matriz = await _service.ObterMatrizIndividualAsync(usuarioId, RoleUsuario.Vendedor);

        matriz.Single(m => m.Chave == ChaveFornecedores).OverrideIndividual.Should().BeTrue();
    }

    // ==================== AtualizarIndividualAsync ====================

    [Fact]
    public async Task AtualizarIndividualAsync_ChaveDesconhecida_Falha()
    {
        var resultado = await _service.AtualizarIndividualAsync(Guid.NewGuid(), "pagina:/nao-existe", true);
        resultado.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AtualizarIndividualAsync_SemRegistroExistente_Cria()
    {
        var usuarioId = Guid.NewGuid();

        var resultado = await _service.AtualizarIndividualAsync(usuarioId, ChaveNovoFornecedor, true);

        resultado.IsSuccess.Should().BeTrue();
        _repoIndividual.Verify(r => r.AddAsync(It.Is<PermissaoIndividual>(
            p => p.UsuarioId == usuarioId && p.RecursoChave == ChaveNovoFornecedor && p.Permitido)), Times.Once);
        _repoIndividual.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AtualizarIndividualAsync_ComRegistroExistente_Atualiza()
    {
        var usuarioId = Guid.NewGuid();
        var existente = new PermissaoIndividual(usuarioId, ChaveNovoFornecedor, false);
        _repoIndividual.Setup(r => r.GetAsync(usuarioId, ChaveNovoFornecedor)).ReturnsAsync(existente);

        var resultado = await _service.AtualizarIndividualAsync(usuarioId, ChaveNovoFornecedor, true);

        resultado.IsSuccess.Should().BeTrue();
        existente.Permitido.Should().BeTrue();
        _repoIndividual.Verify(r => r.Update(existente), Times.Once);
    }

    [Fact]
    public async Task AtualizarIndividualAsync_PermitidoNulo_ComRegistroExistente_Remove()
    {
        var usuarioId = Guid.NewGuid();
        var existente = new PermissaoIndividual(usuarioId, ChaveNovoFornecedor, true);
        _repoIndividual.Setup(r => r.GetAsync(usuarioId, ChaveNovoFornecedor)).ReturnsAsync(existente);

        var resultado = await _service.AtualizarIndividualAsync(usuarioId, ChaveNovoFornecedor, null);

        resultado.IsSuccess.Should().BeTrue();
        _repoIndividual.Verify(r => r.Remove(existente), Times.Once);
    }

    [Fact]
    public async Task AtualizarIndividualAsync_PermitidoNulo_SemRegistroExistente_NaoFazNada()
    {
        var usuarioId = Guid.NewGuid();

        var resultado = await _service.AtualizarIndividualAsync(usuarioId, ChaveNovoFornecedor, null);

        resultado.IsSuccess.Should().BeTrue();
        _repoIndividual.Verify(r => r.Remove(It.IsAny<PermissaoIndividual>()), Times.Never);
        _repoIndividual.Verify(r => r.AddAsync(It.IsAny<PermissaoIndividual>()), Times.Never);
    }
}
