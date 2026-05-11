using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unidade.Services
{
    public class VendedorServiceTests
    {
        private readonly Mock<IVendedorRepository> _repoMock;
        private readonly VendedorService _service;

        public VendedorServiceTests()
        {
            _repoMock = new Mock<IVendedorRepository>();
            _service = new VendedorService(_repoMock.Object);
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_VendedorExistente_RetornaDTO()
        {
            var vendedor = CriarVendedorValido();
            _repoMock.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);

            var result = await _service.GetByIdAsync(vendedor.Id);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_VendedorInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Vendedor?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_RetornaListaDeDTOs()
        {
            var vendedores = new List<Vendedor> { CriarVendedorValido(), CriarVendedorValido() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(vendedores);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_DTOValido_CriaVendedorERetornaId()
        {
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Vendedor>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new CriarVendedorDTO
            {
                Nome = "Carlos Vendedor",
                Email = "carlos@venda.com",
                Telefone = "11988888888",
                Senha = "Senha@123",
                Nivel = "Pleno",
                DataContratacao = DateTime.Now.AddDays(1)
            };

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Vendedor>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_RepositorioLancaExcecao_RetornaFalha()
        {
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Vendedor>())).ThrowsAsync(new Exception("Erro de persistência"));
            var dto = new CriarVendedorDTO
            {
                Nome = "Falha", Email = "falha@teste.com", Telefone = "11900000000", Senha = "123", Nivel = "Junior", DataContratacao = DateTime.Now.AddDays(1)
            };

            var result = await _service.AddAsync(dto);
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Erro ao criar vendedor");
        }

        // ==================== UpdateAsync ====================

        [Fact]
        public async Task UpdateAsync_VendedorExistente_AtualizaESalva()
        {
            var vendedor = CriarVendedorValido();
            _repoMock.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarVendedorDTO { Id = vendedor.Id, Email = "novo@email.com", Telefone = "11912345678" };

            var result = await _service.UpdateAsync(dto);
            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Update(vendedor), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_VendedorInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Vendedor?)null);
            var result = await _service.UpdateAsync(new AtualizarVendedorDTO { Id = Guid.NewGuid() });
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== RemoveAsync ====================

        [Fact]
        public async Task RemoveAsync_VendedorExistente_RemoveESalva()
        {
            var vendedor = CriarVendedorValido();
            _repoMock.Setup(r => r.GetByIdAsync(vendedor.Id)).ReturnsAsync(vendedor);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoveAsync(vendedor.Id);
            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Remove(vendedor), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_VendedorInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Vendedor?)null);
            var result = await _service.RemoveAsync(Guid.NewGuid());
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== MÉTODO AUXILIAR ====================

        private static Vendedor CriarVendedorValido()
        {
            var vendedor = new Vendedor(
                "Vendedor Teste",
                "vendedor@teste.com",
                "11999999999",
                "Senha123",
                3000,
                NivelFuncionario.Junior,
                DateTime.Now.AddDays(1)
            );
            typeof(Vendedor).BaseType?.GetProperty("Id")?.SetValue(vendedor, Guid.NewGuid());
            return vendedor;
        }
    }
}