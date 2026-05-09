
//este codigo nao foi revisado nem documentado
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;
using CarStoreManager.Domain.Base;

namespace CarStoreManager.Tests.Unidade.Services.Concessionaria;

public class VeiculoVendaServiceTests
{
    private readonly Mock<IVeiculoVendaRepository> _repoMock;
    private readonly VeiculoVendaService _service;

    public VeiculoVendaServiceTests()
    {
        _repoMock = new Mock<IVeiculoVendaRepository>();
        _service = new VeiculoVendaService(_repoMock.Object);
    }

    // ==================== GetByIdAsync ====================

    [Fact]
    public async Task GetByIdAsync_VeiculoExistente_RetornaDTO()
    {
        var veiculo = CriarVeiculoValido();
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);

        var result = await _service.GetByIdAsync(veiculo.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== AddAsync ====================

    [Fact]
    public async Task AddAsync_DTOValido_AdicionaERetornaId()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<VeiculoVenda>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var dto = new CriarVeiculoVendaDTO
        {
            Marca = "Honda", Modelo = "Fit", Cor = "Vermelho", Motorizacao = "1.4",
            Ano = 2021, Quilometragem = 12000, Placa = "XYZ1A23",
            Cambio = "Automatico", Combustivel = "Flex", Valor = 60000.00m
        };

        var result = await _service.AddAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<VeiculoVenda>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_RepositorioLancaExcecao_RetornaFalha()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<VeiculoVenda>())).ThrowsAsync(new Exception("erro de persistência"));
        var dto = new CriarVeiculoVendaDTO
        {
            Marca = "Marca", Modelo = "Modelo", Cor = "Cor", Motorizacao = "1.0",
            Ano = 2020, Quilometragem = 100, Placa = "ABC1234",
            Cambio = "Manual", Combustivel = "Gasolina", Valor = 10000
        };

        var result = await _service.AddAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro ao criar veículo");
    }

    // ==================== UpdateAsync ====================

    [Fact]
    public async Task UpdateAsync_VeiculoExistente_AtualizaESalva()
    {
        var veiculo = CriarVeiculoValido();
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var dto = new AtualizarVeiculoVendaDTO { 
            Id = veiculo.Id, 
            Valor = veiculo.GetValor(),
            Disponibilidade = veiculo.Disponibilidade.ToString()
            };

        var result = await _service.UpdateAsync(dto);
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.Update(veiculo), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.UpdateAsync(new AtualizarVeiculoVendaDTO { Id = Guid.NewGuid() });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== MarcarComoVendidoAsync ====================

    [Fact]
    public async Task MarcarComoVendidoAsync_VeiculoExistente_MarcaVendidoESalva()
    {
        var veiculo = CriarVeiculoValido();
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.MarcarComoVendidoAsync(veiculo.Id);
        result.IsSuccess.Should().BeTrue();
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Vendido);
        _repoMock.Verify(r => r.Update(veiculo), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarcarComoVendidoAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.MarcarComoVendidoAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== MarcarComoDisponivelAsync ====================

    [Fact]
    public async Task MarcarComoDisponivelAsync_VeiculoExistente_MarcaDisponivel()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.MarcarComoVendido(); // estado inicial vendido
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.MarcarComoDisponivelAsync(veiculo.Id);
        result.IsSuccess.Should().BeTrue();
        veiculo.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Disponivel);
    }

    // ==================== AtualizarQuilometragemAsync ====================

    [Fact]
    public async Task AtualizarQuilometragemAsync_VeiculoExistente_AtualizaKm()
    {
        var veiculo = CriarVeiculoValido();
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AtualizarQuilometragemAsync(veiculo.Id, 25000);

        Assert.Equal(25000, veiculo.GetQuilometragem());
        _repoMock.Verify(r => r.Update(veiculo), Times.Once);
    }

    [Fact]
    public async Task AtualizarQuilometragemAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.AtualizarQuilometragemAsync(Guid.NewGuid(), 100);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== AdicionarFotoAsync ====================

    [Fact]
    public async Task AdicionarFotoAsync_VeiculoExistente_AdicionaFoto()
    {
        var veiculo = CriarVeiculoValido();
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AdicionarFotoAsync(veiculo.Id, "http://imagens.com/foto.jpg");
        result.IsSuccess.Should().BeTrue();
        veiculo.Fotos.Should().ContainSingle();
        veiculo.Fotos[0].Url.Should().Be("http://imagens.com/foto.jpg");
    }

    [Fact]
    public async Task AdicionarFotoAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.AdicionarFotoAsync(Guid.NewGuid(), "url");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== RemoverFotoAsync ====================

    [Fact]
    public async Task RemoverFotoAsync_FotoExistente_RemoveESalva()
    {
        var veiculo = CriarVeiculoValido();
        veiculo.AdicionarFoto("foto1.jpg");
        var fotoId = veiculo.Fotos[0].Id;
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RemoverFotoAsync(veiculo.Id, fotoId);
        result.IsSuccess.Should().BeTrue();
        veiculo.Fotos.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoverFotoAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.RemoverFotoAsync(Guid.NewGuid(), Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== RemoveAsync ====================

    [Fact]
    public async Task RemoveAsync_VeiculoExistente_RemoveEConfirma()
    {
        var veiculo = CriarVeiculoValido();
        _repoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RemoveAsync(veiculo.Id);
        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.Remove(veiculo), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_VeiculoInexistente_RetornaFalha()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoVenda?)null);
        var result = await _service.RemoveAsync(Guid.NewGuid());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("não encontrado");
    }

    // ==================== MÉTODO AUXILIAR ====================

    private static VeiculoVenda CriarVeiculoValido()
    {
        var veiculo = new VeiculoVenda(
            "Chevrolet", "Onix", "Branco", "1.0 Turbo",
            2022, 20000, "DEF5G67",
            TipoCambio.Manual, TipoCombustivel.Gasolina,
            70000m, AcessoriosVeiculo.VidrosEletricos);
        // Definir um Id para a entidade (simulado)
        typeof(Entity).GetProperty("Id")?.SetValue(veiculo, Guid.NewGuid());
        return veiculo;
    }
}
// Nota: Caso o enum AcessoriosVeiculo não seja utilizado no mapeamento, pode ser omitido.