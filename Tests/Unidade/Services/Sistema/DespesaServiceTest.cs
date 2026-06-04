using FluentAssertions;
using Moq;

using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Tests.Unidade.Services.Sistema;

/// <summary>
/// Testes unitários do DespesaService.
///
/// O service é fino — converte DTOs ↔ entidade, valida o tipo informado e
/// soma valores. As regras (nome obrigatório, valor não-negativo, etc.) ficam
/// na entidade Despesa, então aqui o foco é:
///   • mapeamento correto entre DTO e entidade (incluindo TipoDespesaId/TipoNome),
///   • rejeição quando o tipo informado não existe,
///   • transições Ativa/Desativa quando o DTO de update muda o flag,
///   • soma do total mensal apenas com despesas ativas.
/// </summary>
public class DespesaServiceTests
{
    private readonly Mock<IDespesaRepository> _repoMock = new();
    private readonly Mock<ITipoDespesaRepository> _tipoRepoMock = new();
    private readonly DespesaService _service;

    private readonly TipoDespesa _tipo = new("Aluguel");

    public DespesaServiceTests()
    {
        _service = new DespesaService(_repoMock.Object, _tipoRepoMock.Object);
        // Por padrão, o tipo usado nos testes existe.
        _tipoRepoMock.Setup(r => r.GetByIdAsync(_tipo.Id)).ReturnsAsync(_tipo);
        _tipoRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { _tipo });
    }

    // ==================== ADD ====================

    [Fact]
    public async Task AddAsync_DTOValido_PersisteESalvaERetornaId()
    {
        var dto = new CriarDespesaDTO { Nome = "Aluguel", Valor = 1500m, TipoDespesaId = _tipo.Id };

        var resultado = await _service.AddAsync(dto);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.Is<Despesa>(
            d => d.Nome == "Aluguel" && d.GetValor() == 1500m && d.TipoDespesaId == _tipo.Id)), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_TipoInexistente_RetornaFailSemSalvar()
    {
        _tipoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TipoDespesa?)null);
        var dto = new CriarDespesaDTO { Nome = "Luz", Valor = 200m, TipoDespesaId = Guid.NewGuid() };

        var resultado = await _service.AddAsync(dto);

        resultado.IsSuccess.Should().BeFalse();
        resultado.Error.Should().Contain("tipo");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Despesa>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_NomeVazio_RetornaFail()
    {
        // Regra vem da entidade Despesa, service só repassa
        var dto = new CriarDespesaDTO { Nome = "", Valor = 100m, TipoDespesaId = _tipo.Id };

        var resultado = await _service.AddAsync(dto);

        resultado.IsSuccess.Should().BeFalse();
        resultado.Error.Should().Contain("Nome");
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ValorNegativo_RetornaFail()
    {
        var dto = new CriarDespesaDTO { Nome = "Luz", Valor = -50m, TipoDespesaId = _tipo.Id };

        var resultado = await _service.AddAsync(dto);

        resultado.IsSuccess.Should().BeFalse();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Despesa>()), Times.Never);
    }

    // ==================== GET ====================

    [Fact]
    public async Task GetByIdAsync_Existente_RetornaDTOComTipoNome()
    {
        var despesa = new Despesa("Internet", 100m, _tipo.Id);
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);

        var resultado = await _service.GetByIdAsync(despesa.Id);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value!.Nome.Should().Be("Internet");
        resultado.Value.Valor.Should().Be(100m);
        resultado.Value.TipoDespesaId.Should().Be(_tipo.Id);
        resultado.Value.TipoNome.Should().Be("Aluguel");
        resultado.Value.Ativa.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaFail()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Despesa?)null);

        var resultado = await _service.GetByIdAsync(Guid.NewGuid());

        resultado.IsSuccess.Should().BeFalse();
        resultado.Error.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task GetAllAsync_RetornaTodasComoDTOs()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new[]
        {
            new Despesa("a", 10m, _tipo.Id), new Despesa("b", 20m, _tipo.Id)
        });

        var resultado = await _service.GetAllAsync();

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().HaveCount(2);
    }

    // ==================== UPDATE ====================

    [Fact]
    public async Task UpdateAsync_Inexistente_RetornaFail()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Despesa?)null);

        var r = await _service.UpdateAsync(new AtualizarDespesaDTO { Id = Guid.NewGuid(), TipoDespesaId = _tipo.Id });

        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Existente_AlteraNomeValorETipo()
    {
        var outroTipo = new TipoDespesa("Energia");
        _tipoRepoMock.Setup(r => r.GetByIdAsync(outroTipo.Id)).ReturnsAsync(outroTipo);

        var despesa = new Despesa("antigo", 10m, _tipo.Id);
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);

        var r = await _service.UpdateAsync(new AtualizarDespesaDTO
        {
            Id = despesa.Id, Nome = "novo", Valor = 50m, TipoDespesaId = outroTipo.Id, Ativa = true
        });

        r.IsSuccess.Should().BeTrue();
        despesa.Nome.Should().Be("novo");
        despesa.GetValor().Should().Be(50m);
        despesa.TipoDespesaId.Should().Be(outroTipo.Id);
        _repoMock.Verify(repo => repo.Update(despesa), Times.Once);
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_TipoInexistente_RetornaFailSemSalvar()
    {
        var despesa = new Despesa("X", 10m, _tipo.Id);
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);
        _tipoRepoMock.Setup(r => r.GetByIdAsync(It.Is<Guid>(g => g != _tipo.Id))).ReturnsAsync((TipoDespesa?)null);

        var r = await _service.UpdateAsync(new AtualizarDespesaDTO
        {
            Id = despesa.Id, Nome = "X", Valor = 10m, TipoDespesaId = Guid.NewGuid(), Ativa = true
        });

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DesativaQuandoFlagFalsa()
    {
        var despesa = new Despesa("X", 10m, _tipo.Id); // Ativa = true por padrão
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);

        await _service.UpdateAsync(new AtualizarDespesaDTO
        {
            Id = despesa.Id, Nome = "X", Valor = 10m, TipoDespesaId = _tipo.Id, Ativa = false
        });

        despesa.Ativa.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ReativaQuandoFlagVerdadeira()
    {
        var despesa = new Despesa("X", 10m, _tipo.Id);
        despesa.Desativar();
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);

        await _service.UpdateAsync(new AtualizarDespesaDTO
        {
            Id = despesa.Id, Nome = "X", Valor = 10m, TipoDespesaId = _tipo.Id, Ativa = true
        });

        despesa.Ativa.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NomeVazio_RetornaFail()
    {
        var despesa = new Despesa("nome ok", 10m, _tipo.Id);
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);

        var r = await _service.UpdateAsync(new AtualizarDespesaDTO
        {
            Id = despesa.Id, Nome = "", Valor = 10m, TipoDespesaId = _tipo.Id, Ativa = true
        });

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    // ==================== REMOVE ====================

    [Fact]
    public async Task RemoveAsync_Existente_RemoveESalva()
    {
        var despesa = new Despesa("X", 10m, _tipo.Id);
        _repoMock.Setup(r => r.GetByIdAsync(despesa.Id)).ReturnsAsync(despesa);

        var r = await _service.RemoveAsync(despesa.Id);

        r.IsSuccess.Should().BeTrue();
        _repoMock.Verify(repo => repo.Remove(despesa), Times.Once);
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Inexistente_RetornaFail()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Despesa?)null);

        var r = await _service.RemoveAsync(Guid.NewGuid());

        r.IsSuccess.Should().BeFalse();
        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    // ==================== TOTAL MENSAL ====================

    [Fact]
    public async Task ObterTotalMensalAsync_SomaApenasAtivas()
    {
        // A entidade retorna apenas ativas via GetAtivasAsync — testamos que o service
        // soma os valores corretamente, sem reincluir desativadas
        _repoMock.Setup(r => r.GetAtivasAsync()).ReturnsAsync(new[]
        {
            new Despesa("luz", 100m, _tipo.Id),
            new Despesa("água", 50m, _tipo.Id),
            new Despesa("aluguel", 1500m, _tipo.Id)
        });

        var r = await _service.ObterTotalMensalAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(1650m);
    }

    [Fact]
    public async Task ObterTotalMensalAsync_SemDespesas_RetornaZero()
    {
        _repoMock.Setup(r => r.GetAtivasAsync()).ReturnsAsync(Array.Empty<Despesa>());

        var r = await _service.ObterTotalMensalAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(0m);
    }
}
