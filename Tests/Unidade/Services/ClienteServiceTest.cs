using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Shared;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Tests.Unit.Application
{
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _repoMock;
        private readonly ClienteService _service;

        public ClienteServiceTests()
        {
            _repoMock = new Mock<IClienteRepository>();
            _service = new ClienteService(_repoMock.Object);
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_ClienteExistente_RetornaDTO()
        {
            var cliente = CriarClienteValido();
            _repoMock.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);

            var result = await _service.GetByIdAsync(cliente.Id);

            result.IsSuccess.Should().BeTrue();
            //result.Value.Nome.Should().Be(cliente.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_ClienteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_RetornaListaDeDTOs()
        {
            var clientes = new List<Cliente> { CriarClienteValido(), CriarClienteValido() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clientes);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        // ==================== ObterPorCpfAsync ====================

        [Fact]
        public async Task ObterPorCpfAsync_ClienteExistente_RetornaDTO()
        {
            var cliente = CriarClienteValido();
            var cpf = cliente.GetCpf();
            _repoMock.Setup(r => r.ObterPorCpfAsync(cpf)).ReturnsAsync(cliente);

            var result = await _service.ObterPorCpfAsync(cpf);

            result.IsSuccess.Should().BeTrue();
            //result.Value.Cpf.Should().Be(cpf);
        }

        [Fact]
        public async Task ObterPorCpfAsync_ClienteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.ObterPorCpfAsync(It.IsAny<string>())).ReturnsAsync((Cliente?)null);

            var result = await _service.ObterPorCpfAsync("12345678901");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== PesquisarAsync ====================

        [Fact]
        public async Task PesquisarAsync_TermoExistente_RetornaListaFiltrada()
        {
            var clientes = new List<Cliente> { CriarClienteValido("João"), CriarClienteValido("Maria") };
            _repoMock.Setup(r => r.PesquisarAsync("João")).ReturnsAsync(clientes.Where(c => c.Nome.Contains("João")).ToList());

            var result = await _service.PesquisarAsync("João");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().ContainSingle();
            //result.Value[0].Nome.Should().Be("João");
        }

        [Fact]
        public async Task PesquisarAsync_RepositorioLancaExcecao_RetornaFalha()
        {
            _repoMock.Setup(r => r.PesquisarAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Erro de banco"));

            var result = await _service.PesquisarAsync("teste");

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Erro na pesquisa");
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_CPFJaCadastrado_RetornaFalha()
        {
            var dto = new CriarClienteDTO { Nome = "Novo", Email = "novo@email.com", Telefone = "11988888888", Cpf = "12345678901" };
            _repoMock.Setup(r => r.CpfExisteAsync(dto.Cpf)).ReturnsAsync(true);

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("CPF já cadastrado");
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_DTOValido_CriaClienteERetornaId()
        {
            var dto = new CriarClienteDTO
            {
                Nome = "Carlos",
                Email = "carlos@email.com",
                Telefone = "11999999999",
                Cpf = "52998224725" // CPF válido
            };
            _repoMock.Setup(r => r.CpfExisteAsync(dto.Cpf)).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Cliente>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Cliente>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_DTOComNomeVazio_RetornaFalha()
        {
            var dto = new CriarClienteDTO
            {
                Nome = "",
                Email = "invalido@email.com",
                Telefone = "11900000000",
                Cpf = "52998224725"
            };
            _repoMock.Setup(r => r.CpfExisteAsync(dto.Cpf)).ReturnsAsync(false);

            var result = await _service.AddAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Erro ao criar cliente");
        }

        // ==================== UpdateAsync ====================

        [Fact]
        public async Task UpdateAsync_ClienteExistente_AtualizaESalva()
        {
            var cliente = CriarClienteValido();
            _repoMock.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new AtualizarClienteDTO
            {
                Id = cliente.Id,
                Nome = "Nome Atualizado",
                Email = "novo@email.com",
                Telefone = "11912345678"
            };

            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeTrue();
            cliente.Nome.Should().Be("Nome Atualizado");
            _repoMock.Verify(r => r.Update(cliente), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ClienteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);
            var dto = new AtualizarClienteDTO { Id = Guid.NewGuid() };

            var result = await _service.UpdateAsync(dto);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== RemoveAsync ====================

        [Fact]
        public async Task RemoveAsync_ClienteExistente_RemoveESalva()
        {
            var cliente = CriarClienteValido();
            _repoMock.Setup(r => r.GetByIdAsync(cliente.Id)).ReturnsAsync(cliente);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.RemoveAsync(cliente.Id);

            result.IsSuccess.Should().BeTrue();
            _repoMock.Verify(r => r.Remove(cliente), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ClienteInexistente_RetornaFalha()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cliente?)null);

            var result = await _service.RemoveAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("não encontrado");
        }

        // ==================== MÉTODO AUXILIAR ====================

        private static Cliente CriarClienteValido(string nome = "Cliente Teste")
        {
            // O construtor de Cliente espera (nome, email, telefone, cpf)
            var cliente = new Cliente(
                nome,
                "cliente@teste.com",
                "11999999999",
                "52998224725" // CPF válido
            );
            typeof(Cliente).BaseType?.GetProperty("Id")?.SetValue(cliente, Guid.NewGuid());
            return cliente;
        }
    }
}