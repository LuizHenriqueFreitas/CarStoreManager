using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unit.Application
{
    public class VeiculoClienteServiceTests
    {
        private readonly Mock<IVeiculoClienteRepository> _veiculoRepoMock;
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly VeiculoClienteService _service;

        public VeiculoClienteServiceTests()
        {
            _veiculoRepoMock = new Mock<IVeiculoClienteRepository>();
            _clienteRepoMock = new Mock<IClienteRepository>();
            _service = new VeiculoClienteService(_veiculoRepoMock.Object, _clienteRepoMock.Object);
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_VeiculoExistente_RetornaDTO()
        {
            var veiculo = CriarVeiculoValido();
            _veiculoRepoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);

            var result = await _service.GetByIdAsync(veiculo.Id);

            result.IsSuccess.Should().BeTrue();
            //result.Value.Marca.Should().Be(veiculo.Marca);
            //result.Value.Modelo.Should().Be(veiculo.Modelo);
        }

        [Fact]
        public async Task GetByIdAsync_VeiculoInexistente_RetornaFalha()
        {
            _veiculoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoCliente?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_RetornaListaDeDTOs()
        {
            var veiculos = new List<VeiculoCliente> { CriarVeiculoValido(), CriarVeiculoValido() };
            _veiculoRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(veiculos);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== ObterPorClienteAsync ====================

        [Fact]
        public async Task ObterPorClienteAsync_ClienteExistente_RetornaLista()
        {
            var clienteId = Guid.NewGuid();
            var veiculos = new List<VeiculoCliente> { CriarVeiculoValido(clienteId), CriarVeiculoValido(clienteId) };
            var clienteMock = new Mock<Cliente>();
            _clienteRepoMock.Setup(r => r.GetByIdAsync(clienteId)).ReturnsAsync(clienteMock.Object);
            _veiculoRepoMock.Setup(r => r.ObterPorClienteAsync(clienteId)).ReturnsAsync(veiculos);

            var result = await _service.ObterPorClienteAsync(clienteId);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObterPorClienteAsync_ClienteInexistente_RetornaFalha()
        {
            _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

            var result = await _service.ObterPorClienteAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Cliente não encontrado");
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_ClienteExistenteEDTOValido_CriaVeiculoERetornaId()
        {
            var clienteMock = new Mock<Cliente>();
            var dto = new CriarVeiculoClienteDTO
            {
                ClienteId = Guid.NewGuid(),
                Marca = "Fiat",
                Modelo = "Uno",
                Cor = "Vermelho",
                Ano = 2015
            };
            _clienteRepoMock.Setup(r => r.GetByIdAsync(dto.ClienteId)).ReturnsAsync(clienteMock.Object);
            _veiculoRepoMock.Setup(r => r.AddAsync(It.IsAny<VeiculoCliente>())).Returns(Task.CompletedTask);
            _veiculoRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            _veiculoRepoMock.Verify(r => r.AddAsync(It.IsAny<VeiculoCliente>()), Times.Once);
            _veiculoRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ClienteInexistente_RetornaFalha()
        {
            _clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
            var dto = new CriarVeiculoClienteDTO { ClienteId = Guid.NewGuid(), Marca = "", Modelo = "", Cor = "", Ano = 0 };

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Cliente não encontrado");
        }

        [Fact]
        public async Task AddAsync_DTOComMarcaVazia_RetornaFalha()
        {
            var clienteMock = new Mock<Cliente>();
            var dto = new CriarVeiculoClienteDTO
            {
                ClienteId = Guid.NewGuid(),
                Marca = "",
                Modelo = "Uno",
                Cor = "Azul",
                Ano = 2010
            };
            _clienteRepoMock.Setup(r => r.GetByIdAsync(dto.ClienteId)).ReturnsAsync(clienteMock.Object);

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Erro ao cadastrar veículo");
        }

        // ==================== UpdateAsync ====================

        [Fact]
        public async Task UpdateAsync_VeiculoExistente_AtualizaESalva()
        {
            var veiculo = CriarVeiculoValido();
            _veiculoRepoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
            _veiculoRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarVeiculoClienteDTO
            {
                Id = veiculo.Id,
                Marca = "Chevrolet",
                Modelo = "Onix",
                Cor = "Branco",
                Ano = 2022
            };

            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeTrue();
            veiculo.GetMarca().Should().Be("Chevrolet");
            veiculo.GetModelo().Should().Be("Onix");
            veiculo.GetCor().Should().Be("Branco");
            veiculo.GetAno().Should().Be(2022);
            _veiculoRepoMock.Verify(r => r.Update(veiculo), Times.Once);
            _veiculoRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_VeiculoInexistente_RetornaFalha()
        {
            _veiculoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoCliente?)null);
            var dto = new AtualizarVeiculoClienteDTO { Id = Guid.NewGuid() };

            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== RemoveAsync ====================

        [Fact]
        public async Task RemoveAsync_VeiculoExistente_RemoveESalva()
        {
            var veiculo = CriarVeiculoValido();
            _veiculoRepoMock.Setup(r => r.GetByIdAsync(veiculo.Id)).ReturnsAsync(veiculo);
            _veiculoRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoveAsync(veiculo.Id);

            result.IsSuccess.Should().BeTrue();
            _veiculoRepoMock.Verify(r => r.Remove(veiculo), Times.Once);
            _veiculoRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_VeiculoInexistente_RetornaFalha()
        {
            _veiculoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VeiculoCliente?)null);

            var result = await _service.RemoveAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== MÉTODO AUXILIAR ====================

        private static VeiculoCliente CriarVeiculoValido(Guid? clienteId = null)
        {
            var veiculo = new VeiculoCliente(
                clienteId ?? Guid.NewGuid(),
                "Honda",
                "Civic",
                "Preto",
                2020
            );
            typeof(VeiculoCliente).BaseType?.GetProperty("Id")?.SetValue(veiculo, Guid.NewGuid());
            return veiculo;
        }
    }
}