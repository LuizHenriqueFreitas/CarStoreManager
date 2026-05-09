using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories
{
    public class ComponenteRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly ComponenteRepository _repository;

        public ComponenteRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new ComponenteRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_ComponenteExistente_RetornaComponente()
        {
            var componente = await SalvarComponente("Filtro de óleo", "F-01", SistemaComponente.Motor, 50.00m, 10, 2);

            var resultado = await _repository.GetByIdAsync(componente.Id);

            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be("Filtro de óleo");
            resultado.Modelo.Should().Be("F-01");
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
            await SalvarComponente("Pastilha", "P-100", SistemaComponente.Freios, 150.00m, 5, 1);
            await SalvarComponente("Óleo", "O-20", SistemaComponente.Motor, 40.00m, 0, 3);

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
        }

        // ==================== ObterComEstoqueBaixoAsync ====================

        [Fact]
        public async Task ObterComEstoqueBaixoAsync_ComItensAbaixoDoMinimo_RetornaFiltrados()
        {
            await SalvarComponente("Item OK", "M1", SistemaComponente.Motor, 10, 10, 2);     // estoque > mínimo
            await SalvarComponente("Item Baixo", "M2", SistemaComponente.Motor, 10, 1, 5);   // estoque < mínimo
            await SalvarComponente("Item Igual", "M3", SistemaComponente.Motor, 10, 3, 3);    // estoque == mínimo

            var resultado = await _repository.ObterComEstoqueBaixoAsync();

            resultado.Should().HaveCount(2);
            resultado.All(c => c.QuantidadeEstoque <= c.EstoqueMinimo).Should().BeTrue();
        }

        [Fact]
        public async Task ObterComEstoqueBaixoAsync_SemItensBaixos_RetornaVazio()
        {
            await SalvarComponente("Item OK", "M1", SistemaComponente.Motor, 10, 10, 2);

            var resultado = await _repository.ObterComEstoqueBaixoAsync();
            resultado.Should().BeEmpty();
        }

        // ==================== ObterPorSistemaAsync ====================

        [Fact]
        public async Task ObterPorSistemaAsync_SistemaExistente_RetornaFiltrados()
        {
            await SalvarComponente("Freio", "F1", SistemaComponente.Freios, 100, 5, 2);
            await SalvarComponente("Motor", "M1", SistemaComponente.Motor, 200, 3, 1);
            await SalvarComponente("Freio2", "F2", SistemaComponente.Freios, 120, 8, 3);

            var resultado = await _repository.ObterPorSistemaAsync(SistemaComponente.Freios);
            resultado.Should().HaveCount(2);
            resultado.All(c => c.Sistema == SistemaComponente.Freios).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorSistemaAsync_SistemaSemComponentes_RetornaVazio()
        {
            var resultado = await _repository.ObterPorSistemaAsync(SistemaComponente.Eletrica);
            resultado.Should().BeEmpty();
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_ComponenteValido_PersisteCorretamente()
        {
            var componente = new Componente("Correia", "C-10", SistemaComponente.Motor, 80.00m, 4, 1);
            await _repository.AddAsync(componente);
            await _repository.SaveChangesAsync();

            var salvo = await _repository.GetByIdAsync(componente.Id);
            salvo.Should().NotBeNull();
            salvo!.Nome.Should().Be("Correia");
            salvo.Modelo.Should().Be("C-10");
            salvo.Sistema.Should().Be(SistemaComponente.Motor);
            salvo.Valor.GetValorDinheiro().Should().Be(80.00m);
            salvo.QuantidadeEstoque.Should().Be(4);
            salvo.EstoqueMinimo.Should().Be(1);
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_ComponenteExistente_AtualizaDados()
        {
            var componente = await SalvarComponente("Original", "O-1", SistemaComponente.Motor, 90.00m, 3, 2);

            componente.DefinirNome("Atualizado");
            componente.AdicionarEstoque(5); // 3 + 5 = 8
            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            var atualizado = await _repository.GetByIdAsync(componente.Id);
            atualizado!.Nome.Should().Be("Atualizado");
            atualizado.QuantidadeEstoque.Should().Be(8);
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_ComponenteExistente_RemoveRegistro()
        {
            var componente = await SalvarComponente("Remover", "R-1", SistemaComponente.Suspensao, 60.00m, 2, 1);

            _repository.Remove(componente);
            await _repository.SaveChangesAsync();

            var removido = await _repository.GetByIdAsync(componente.Id);
            removido.Should().BeNull();
        }

        // ==================== Método auxiliar ====================

        private async Task<Componente> SalvarComponente(
            string nome,
            string modelo,
            SistemaComponente sistema,
            decimal valor,
            int quantidade,
            int estoqueMinimo)
        {
            var componente = new Componente(nome, modelo, sistema, valor, quantidade, estoqueMinimo);
            await _repository.AddAsync(componente);
            await _repository.SaveChangesAsync();
            return componente;
        }
    }
}