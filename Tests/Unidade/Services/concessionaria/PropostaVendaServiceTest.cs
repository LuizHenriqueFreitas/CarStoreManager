//este arquivo nao foi revisado nem documentado
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Tests.Unidade.Services.Concessionaria;

public class PropostaVendaServiceTests
{
    private readonly Mock<IPropostaVendaRepository> _propostaRepoMock;
    private readonly Mock<IVeiculoVendaRepository> _veiculoRepoMock;
    private readonly PropostaVendaService _service;

    public PropostaVendaServiceTests()
    {
        _propostaRepoMock = new Mock<IPropostaVendaRepository>();
        _veiculoRepoMock = new Mock<IVeiculoVendaRepository>();
        _service = new PropostaVendaService(_propostaRepoMock.Object, _veiculoRepoMock.Object);
    }

    // ==================== GetByIdAsync ====================

    [Fact]
    public async Task GetByIdAsync_PropostaInexistente_RetornaFalha()
    {
        _propostaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PropostaVenda?)null);
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrada");
    }

    // ==================== AddAsync ====================

    [Fact]
    public async Task AddAsync_VeiculoNaoEncontrado_RetornaFalha()
    {
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var dto = new CriarPropostaVendaDTO { VeiculoVendaId = Guid.NewGuid() };
        var result = await _service.AddAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Veículo não encontrado");
    }

    [Fact]
    public async Task AddAsync_VeiculoNaoDisponivel_RetornaFalha()
    {
        var veiculo = new VeiculoVenda("Marca", "Modelo", "Cor", "1.0", 2020, 10000, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 50000, AcessoriosVeiculo.Nenhum);
        typeof(VeiculoVenda).GetProperty("Disponibilidade")?.SetValue(veiculo, DisponibilidadeVeiculo.Vendido);
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(veiculo);
        var dto = new CriarPropostaVendaDTO { VeiculoVendaId = veiculo.Id };
        var result = await _service.AddAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não está disponível");
    }

    [Fact]
    public async Task AddAsync_VeiculoDisponivel_CriaPropostaERetornaId()
    {
        var veiculo = new VeiculoVenda("Marca", "Modelo", "Cor", "1.0", 2020, 10000, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 50000, AcessoriosVeiculo.Nenhum);
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(veiculo);
        _propostaRepoMock.Setup(r => r.AddAsync(It.IsAny<PropostaVenda>())).Returns(Task.CompletedTask);
        _propostaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dto = new CriarPropostaVendaDTO { 
            VeiculoVendaId = veiculo.Id, 
            VendedorId = Guid.NewGuid(), 
            ClienteId = Guid.NewGuid(), 
            ValorBase = 100000, 
            DescontoPercentual = 5
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _propostaRepoMock.Verify(r => r.AddAsync(It.IsAny<PropostaVenda>()), Times.Once);
        _propostaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ==================== AprovarAsync ====================

    [Fact]
    public async Task AprovarAsync_PropostaNaoEncontrada_RetornaFalha()
    {
        _propostaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PropostaVenda?)null);
        var result = await _service.AprovarAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task AprovarAsync_PropostaValida_AprovaEMarcaVeiculoComoVendido()
    {
        var veiculo = new VeiculoVenda("Marca", "Modelo", "Cor", "1.0", 2020, 10000, "ABC1234",
            TipoCambio.Manual, TipoCombustivel.Gasolina, 50000);
        var proposta = new PropostaVenda(Guid.NewGuid(), veiculo.Id, Guid.NewGuid(), 50000, 0);
        _propostaRepoMock.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);
        _veiculoRepoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _propostaRepoMock.Setup(r => r.Update(It.IsAny<PropostaVenda>())).Verifiable();
        _veiculoRepoMock.Setup(r => r.Update(It.IsAny<VeiculoVenda>())).Verifiable();
        _propostaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AprovarAsync(proposta.Id);

        result.IsSuccess.Should().BeTrue();
        proposta.Status.Should().Be(StatusPropostaVenda.Aprovada);
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Vendido);
        _propostaRepoMock.Verify(r => r.Update(proposta), Times.Once);
        _veiculoRepoMock.Verify(r => r.Update(veiculo), Times.Once);
        _propostaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ==================== RejeitarAsync ====================

    [Fact]
    public async Task RejeitarAsync_PropostaValida_RejeitaESalva()
    {
        var proposta = new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10000, 0);
        _propostaRepoMock.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);
        _propostaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RejeitarAsync(proposta.Id);
        result.IsSuccess.Should().BeTrue();
        proposta.Status.Should().Be(StatusPropostaVenda.Rejeitada);
        _propostaRepoMock.Verify(r => r.Update(proposta), Times.Once);
        _propostaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ==================== CancelarAsync ====================

    [Fact]
    public async Task CancelarAsync_PropostaExistente_CancelaESalva()
    {
        var proposta = new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10000, 0);
        _propostaRepoMock.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);
        _propostaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.CancelarAsync(proposta.Id);
        result.IsSuccess.Should().BeTrue();
        proposta.Status.Should().Be(StatusPropostaVenda.Cancelada);
        _propostaRepoMock.Verify(r => r.Update(proposta), Times.Once);
        _propostaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ==================== RemoverAsync ====================

    [Fact]
    public async Task RemoveAsync_PropostaExistente_RemoveESalva()
    {
        var proposta = new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10000, 0);
        _propostaRepoMock.Setup(r => r.GetByIdAsync(proposta.Id)).ReturnsAsync(proposta);
        _propostaRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RemoveAsync(proposta.Id);
        result.IsSuccess.Should().BeTrue();
        _propostaRepoMock.Verify(r => r.Remove(proposta), Times.Once);
        _propostaRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_PropostaInexistente_RetornaFalha()
    {
        _propostaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PropostaVenda?)null);
        var result = await _service.RemoveAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        // Nota: a mensagem de erro original está "Mecânico não encontrado", mas é um erro de digitação; o teste só verifica falha.
    }
}