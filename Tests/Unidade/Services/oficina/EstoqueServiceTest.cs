using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CarStoreManager.Tests.Unidade.Services.Oficina;

public class EstoqueServiceTests
{
    private readonly Mock<IEstoqueRepository> _estoqueRepoMock;
    private readonly Mock<IComponenteRepository> _componenteRepoMock;
    private readonly Mock<IOrdemServicoRepository> _ordemRepoMock;
    private readonly Mock<IBalancoMensalDespesaService> _balancoDespesaMock;
    private readonly EstoqueService _service;

    public EstoqueServiceTests()
    {
        _estoqueRepoMock = new Mock<IEstoqueRepository>();
        _componenteRepoMock = new Mock<IComponenteRepository>();
        _ordemRepoMock = new Mock<IOrdemServicoRepository>();
        _balancoDespesaMock = new Mock<IBalancoMensalDespesaService>();
        _service = new EstoqueService(
            _estoqueRepoMock.Object, _componenteRepoMock.Object, _ordemRepoMock.Object, _balancoDespesaMock.Object);
    }

    // ==================== EntradaAsync ====================

    [Fact]
    public async Task EntradaAsync_ComponenteExistente_LancaDespesaDeCompra()
    {
        var componente = CriarComponenteValido();
        componente.AplicarPrecificacao(custo: 50m, margemPct: 30m);
        _componenteRepoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);
        _estoqueRepoMock.Setup(r => r.ObterPorComponenteAsync(componente.Id)).ReturnsAsync((EstoqueComponente?)null);
        _ordemRepoMock.Setup(r => r.ObterComItensAguardandoAsync(componente.Id)).ReturnsAsync(new List<OrdemServico>());

        SalvarItemBalancoDTO? itemSalvo = null;
        _balancoDespesaMock
            .Setup(b => b.SalvarItemAsync(It.IsAny<SalvarItemBalancoDTO>()))
            .Callback<SalvarItemBalancoDTO>(dto => itemSalvo = dto)
            .ReturnsAsync(Result.Ok());

        var result = await _service.EntradaAsync(componente.Id, quantidade: 10);

        result.IsSuccess.Should().BeTrue();
        _balancoDespesaMock.Verify(b => b.GerarDoModeloAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _balancoDespesaMock.Verify(b => b.SalvarItemAsync(It.IsAny<SalvarItemBalancoDTO>()), Times.Once);
        itemSalvo.Should().NotBeNull();
        itemSalvo!.Setor.Should().Be("Oficina");
        itemSalvo.Categoria.Should().Be("Compra de componentes");
        itemSalvo.Valor.Should().Be(500m); // custo 50 x quantidade 10
    }

    [Fact]
    public async Task EntradaAsync_QuantidadeInvalida_NaoLancaDespesa()
    {
        var result = await _service.EntradaAsync(Guid.NewGuid(), quantidade: 0);

        result.IsSuccess.Should().BeFalse();
        _balancoDespesaMock.Verify(b => b.SalvarItemAsync(It.IsAny<SalvarItemBalancoDTO>()), Times.Never);
    }

    [Fact]
    public async Task EntradaAsync_ComponenteNaoEncontrado_NaoLancaDespesaMasAindaRegistraEntrada()
    {
        var componenteId = Guid.NewGuid();
        _componenteRepoMock.Setup(r => r.GetByIdAsync(componenteId)).ReturnsAsync((Componente?)null);
        _estoqueRepoMock.Setup(r => r.ObterPorComponenteAsync(componenteId)).ReturnsAsync((EstoqueComponente?)null);
        _ordemRepoMock.Setup(r => r.ObterComItensAguardandoAsync(componenteId)).ReturnsAsync(new List<OrdemServico>());

        var result = await _service.EntradaAsync(componenteId, quantidade: 5);

        result.IsSuccess.Should().BeTrue();
        _balancoDespesaMock.Verify(b => b.SalvarItemAsync(It.IsAny<SalvarItemBalancoDTO>()), Times.Never);
    }

    private static Componente CriarComponenteValido()
    {
        var componente = new Componente(
            "FIL-001", "Filtro", "Filtro de óleo", "Bosch", "PN-FIL-1",
            "OEM-1", "7891234567890", "87083010", "0102000",
            "Motor", "UN", 0.3m, 90, Guid.NewGuid());
        typeof(Componente).BaseType?.GetProperty("Id")?.SetValue(componente, Guid.NewGuid());
        return componente;
    }
}
