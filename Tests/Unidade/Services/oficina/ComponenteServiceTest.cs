using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unit.Application
{
    public class ComponenteServiceTests
    {
        private readonly Mock<IComponenteRepository> _repoMock;
        private readonly ComponenteService _service;

        public ComponenteServiceTests()
        {
            _repoMock = new Mock<IComponenteRepository>();
            _service = new ComponenteService(_repoMock.Object);
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_ComponenteExistente_RetornaDTO()
        {
            var componente = CriarComponenteValido();
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);

            var result = await _service.GetByIdAsync(componente.Id);

            result.IsSuccess.Should().BeTrue();
            //result.Value.Nome.Should().Be(componente.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_ComponenteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_RetornaListaDeDTOs()
        {
            var componentes = new List<Componente> { CriarComponenteValido(), CriarComponenteValido() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(componentes);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== ObterComEstoqueBaixoAsync ====================

        [Fact]
        public async Task ObterComEstoqueBaixoAsync_FiltraApenasEstoqueBaixo()
        {
            var componenteBaixo = CriarComponenteValido(quantidade: 2, estoqueMinimo: 5);
            var componenteNormal = CriarComponenteValido(quantidade: 10, estoqueMinimo: 3);
            var componentes = new List<Componente> { componenteBaixo, componenteNormal };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(componentes);

            var result = await _service.ObterComEstoqueBaixoAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            //result.Value.First().Nome.Should().Be(componenteBaixo.Nome);
        }

        // ==================== ObterPorSistemaAsync ====================

        [Fact]
        public async Task ObterPorSistemaAsync_SistemaValido_RetornaFiltrados()
        {
            var motor1 = CriarComponenteValido(sistema: SistemaComponente.Motor);
            var motor2 = CriarComponenteValido(sistema: SistemaComponente.Motor);
            var freio = CriarComponenteValido(sistema: SistemaComponente.Freios);
            var componentes = new List<Componente> { motor1, motor2, freio };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(componentes);

            var result = await _service.ObterPorSistemaAsync("Motor");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObterPorSistemaAsync_SistemaInvalido_RetornaFalha()
        {
            var result = await _service.ObterPorSistemaAsync("Invalido");
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Sistema inválido");
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_DTOValido_CriaComponenteERetornaId()
        {
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Componente>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CriarComponenteDTO
            {
                Nome = "Pastilha",
                Modelo = "P-100",
                Sistema = "Freios",
                Valor = 150.00m,
                QuantidadeEstoque = 10,
                EstoqueMinimo = 5
            };

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Componente>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_SistemaInvalido_RetornaFalha()
        {
            var dto = new CriarComponenteDTO
            {
                Nome = "Teste", Modelo = "X", Sistema = "Invalido", Valor = 10, QuantidadeEstoque = 1, EstoqueMinimo = 0
            };

            var result = await _service.AddAsync(dto);
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Sistema inválido");
        }

        [Fact]
        public async Task AddAsync_DTOComNomeVazio_LancaExcecaoRetornaFalha()
        {
            var dto = new CriarComponenteDTO
            {
                Nome = "", Modelo = "X", Sistema = "Motor", Valor = 10, QuantidadeEstoque = 1, EstoqueMinimo = 0
            };

            var result = await _service.AddAsync(dto);
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Erro ao criar componente");
        }

        // ==================== UpdateAsync ====================

        [Fact]
        public async Task UpdateAsync_ComponenteExistente_AtualizaESalva()
        {
            var componente = CriarComponenteValido();
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarComponenteDTO
            {
                Id = componente.Id,
                Valor = 200,
                QuantidadeEstoque = 20,
                EstoqueMinimo = 8
            };

            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeTrue();
            componente.GetValor().Should().Be(200);
            componente.GetQuantidade().Should().Be(20);
            componente.GetEstoqueMinimo().Should().Be(8);
            _repoMock.Verify(r => r.Update(componente), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ComponenteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);
            var dto = new AtualizarComponenteDTO { Id = Guid.NewGuid() };
            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== EntradaEstoqueAsync ====================

        [Fact]
        public async Task EntradaEstoqueAsync_QuantidadeValida_AdicionaESalva()
        {
            var componente = CriarComponenteValido(quantidade: 10);
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.EntradaEstoqueAsync(componente.Id, 5);

            result.IsSuccess.Should().BeTrue();
            componente.GetQuantidade().Should().Be(15);
            _repoMock.Verify(r => r.Update(componente), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task EntradaEstoqueAsync_QuantidadeNegativa_LancaExcecaoRetornaFalha()
        {
            var componente = CriarComponenteValido();
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);

            var result = await _service.EntradaEstoqueAsync(componente.Id, -1);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EntradaEstoqueAsync_ComponenteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);
            var result = await _service.EntradaEstoqueAsync(Guid.NewGuid(), 10);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== SaidaEstoqueAsync ====================

        [Fact]
        public async Task SaidaEstoqueAsync_QuantidadePossivel_RemoveESalva()
        {
            var componente = CriarComponenteValido(quantidade: 10);
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.SaidaEstoqueAsync(componente.Id, 3);

            result.IsSuccess.Should().BeTrue();
            componente.GetQuantidade().Should().Be(7);
            _repoMock.Verify(r => r.Update(componente), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SaidaEstoqueAsync_EstoqueInsuficiente_RetornaFalha()
        {
            var componente = CriarComponenteValido(quantidade: 4);
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);

            var result = await _service.SaidaEstoqueAsync(componente.Id, 10);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Estoque insuficiente");
        }

        [Fact]
        public async Task SaidaEstoqueAsync_ComponenteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);
            var result = await _service.SaidaEstoqueAsync(Guid.NewGuid(), 1);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== RemoveAsync ====================

        [Fact]
        public async Task RemoveAsync_ComponenteExistente_RemoveESalva()
        {
            var componente = CriarComponenteValido();
            _repoMock.Setup(r => r.GetByIdAsync(componente.Id)).ReturnsAsync(componente);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoveAsync(componente.Id);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Remove(componente), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ComponenteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Componente?)null);
            var result = await _service.RemoveAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== MÉTODO AUXILIAR ====================

        private static Componente CriarComponenteValido(
            string nome = "Filtro de óleo",
            string modelo = "F-01",
            SistemaComponente sistema = SistemaComponente.Motor,
            decimal valor = 50.00m,
            int quantidade = 10,
            int estoqueMinimo = 2)
        {
            var componente = new Componente(nome, modelo, sistema, valor, quantidade, estoqueMinimo);
            typeof(Componente).BaseType?.GetProperty("Id")?.SetValue(componente, Guid.NewGuid());
            return componente;
        }
    }
}