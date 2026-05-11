using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories
{
    public class VeiculoClienteRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly VeiculoClienteRepository _repository;

        public VeiculoClienteRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new VeiculoClienteRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_VeiculoExistente_RetornaVeiculoComHistorico()
        {
            // Arrange
            var veiculo = CriarVeiculoCliente();
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            // Act
            var resultado = await _repository.GetByIdAsync(veiculo.Id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Marca.Should().Be(veiculo.Marca);
            resultado.Modelo.Should().Be(veiculo.Modelo);
            resultado.HistoricoServicos.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_IdInexistente_RetornaNull()
        {
            var resultado = await _repository.GetByIdAsync(Guid.NewGuid());
            resultado.Should().BeNull();
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_SemDados_RetornaListaVazia()
        {
            var resultado = await _repository.GetAllAsync();
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ComDados_RetornaTodos()
        {
            var v1 = CriarVeiculoCliente();
            var v2 = CriarVeiculoCliente();
            await _repository.AddAsync(v1);
            await _repository.AddAsync(v2);
            await _repository.SaveChangesAsync();

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
        }

        // ==================== ObterPorClienteAsync ====================

        [Fact]
        public async Task ObterPorClienteAsync_ClienteComVeiculos_RetornaVeiculos()
        {
            var clienteId = Guid.NewGuid();
            var v1 = CriarVeiculoCliente(clienteId);
            var v2 = CriarVeiculoCliente(clienteId);
            var v3 = CriarVeiculoCliente(); // cliente diferente
            await _repository.AddAsync(v1);
            await _repository.AddAsync(v2);
            await _repository.AddAsync(v3);
            await _repository.SaveChangesAsync();

            var resultado = await _repository.ObterPorClienteAsync(clienteId);
            resultado.Should().HaveCount(2);
            resultado.All(v => v.ClienteId == clienteId).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorClienteAsync_ClienteSemVeiculos_RetornaVazio()
        {
            var resultado = await _repository.ObterPorClienteAsync(Guid.NewGuid());
            resultado.Should().BeEmpty();
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_VeiculoValido_PersisteDados()
        {
            var veiculo = CriarVeiculoCliente();
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            var salvo = await _repository.GetByIdAsync(veiculo.Id);
            salvo.Should().BeEquivalentTo(veiculo, options => options
                .Excluding(v => v.HistoricoServicos));
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_VeiculoExistente_AtualizaDados()
        {
            var veiculo = CriarVeiculoCliente();
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            veiculo.AtualizarDadosVeiculoCliente("NovaMarca", "NovoModelo", "Azul", 2022);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();

            var atualizado = await _repository.GetByIdAsync(veiculo.Id);
            atualizado!.Marca.Should().Be("NovaMarca");
            atualizado.Modelo.Should().Be("NovoModelo");
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_VeiculoExistente_RemoveDoSistema()
        {
            var veiculo = CriarVeiculoCliente();
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            _repository.Remove(veiculo);
            await _repository.SaveChangesAsync();

            var removido = await _repository.GetByIdAsync(veiculo.Id);
            removido.Should().BeNull();
        }

        // ==================== MÉTODO AUXILIAR ====================

        private static VeiculoCliente CriarVeiculoCliente(Guid? clienteId = null)
        {
            return new VeiculoCliente(
                clienteId ?? Guid.NewGuid(),
                "Honda",
                "Civic",
                "Preto",
                2020,
                "ABC1D23"
            );
        }
    }
}