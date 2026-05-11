using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories
{
    public class VendedorRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly VendedorRepository _repository;

        public VendedorRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new VendedorRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_VendedorExistente_RetornaVendedor()
        {
            var vendedor = await SalvarVendedor("Ana", "ana@email.com", "11911111111", NivelFuncionario.Pleno);

            var resultado = await _repository.GetByIdAsync(vendedor.Id);

            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be("Ana");
            resultado.Email.Endereco.Should().Be("ana@email.com");
        }

        [Fact]
        public async Task GetByIdAsync_IdInexistente_RetornaNull()
        {
            var resultado = await _repository.GetByIdAsync(Guid.NewGuid());
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_TipoDiferente_NaoRetornaOutroTipo()
        {
            // Cria um Mecanico (não vendedor) para garantir que OfType filtra
            var mecanico = new Domain.Entities.Oficina.Mecanico("Mec", "mec@email.com", "11922222222", "Senha123", 3000, EspecialidadeMecanico.Mecanica, NivelFuncionario.Junior, DateTime.Now.AddDays(1));
            _context.Usuarios.Add(mecanico);
            await _context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(mecanico.Id);
            resultado.Should().BeNull(); // não é Vendedor
        }

        // ==================== GetAllAsync ====================

        [Fact]
        public async Task GetAllAsync_SemDados_RetornaListaVazia()
        {
            var resultado = await _repository.GetAllAsync();
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ComDados_RetornaApenasVendedores()
        {
            var v1 = await SalvarVendedor("V1", "v1@email.com", "11911111111", NivelFuncionario.Junior);
            var v2 = await SalvarVendedor("V2", "v2@email.com", "11922222222", NivelFuncionario.Senior);
            // adiciona um Mecanico para garantir que não aparece
            var mecanico = new Domain.Entities.Oficina.Mecanico("Mec", "mec@email.com", "11933333333", "Senha123", 3000, EspecialidadeMecanico.Mecanica, NivelFuncionario.Junior, DateTime.Now.AddDays(1));
            _context.Usuarios.Add(mecanico);
            await _context.SaveChangesAsync();

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
            resultado.All(v => v.Role == RoleUsuario.Vendedor).Should().BeTrue();
        }

        // ==================== ObterPorNivelAsync ====================

        [Fact]
        public async Task ObterPorNivelAsync_NivelExistente_RetornaVendedoresFiltrados()
        {
            await SalvarVendedor("Junior1", "j1@email.com", "11911111111", NivelFuncionario.Junior);
            await SalvarVendedor("Pleno1", "p1@email.com", "11922222222", NivelFuncionario.Pleno);
            await SalvarVendedor("Pleno2", "p2@email.com", "11933333333", NivelFuncionario.Pleno);

            var resultado = await _repository.ObterPorNivelAsync(NivelFuncionario.Pleno);
            resultado.Should().HaveCount(2);
            resultado.All(v => v.DadosFuncionario.GetNivel() == NivelFuncionario.Pleno).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorNivelAsync_NivelSemVendedores_RetornaVazio()
        {
            await SalvarVendedor("Junior", "j@email.com", "11911111111", NivelFuncionario.Junior);

            var resultado = await _repository.ObterPorNivelAsync(NivelFuncionario.Senior);
            resultado.Should().BeEmpty();
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_VendedorValido_PersisteCorretamente()
        {
            var vendedor = new Vendedor("Carlos", "carlos@email.com", "11977777777", "Senha123", 3000, NivelFuncionario.Junior, DateTime.Now.AddDays(1));
            await _repository.AddAsync(vendedor);
            await _repository.SaveChangesAsync();

            var salvo = await _repository.GetByIdAsync(vendedor.Id);
            salvo.Should().NotBeNull();
            salvo!.Nome.Should().Be("Carlos");
            salvo.DadosFuncionario.GetNivel().Should().Be(NivelFuncionario.Junior);
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_VendedorExistente_AtualizaDados()
        {
            var vendedor = await SalvarVendedor("Antigo", "antigo@email.com", "11911111111", NivelFuncionario.Junior);

            vendedor.AtualizarNivelVendedor(NivelFuncionario.Senior);
            vendedor.AtualizarNome("Novo Nome");
            _repository.Update(vendedor);
            await _repository.SaveChangesAsync();

            var atualizado = await _repository.GetByIdAsync(vendedor.Id);
            atualizado!.Nome.Should().Be("Novo Nome");
            atualizado.DadosFuncionario.GetNivel().Should().Be(NivelFuncionario.Senior);
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_VendedorExistente_RemoveRegistro()
        {
            var vendedor = await SalvarVendedor("Remover", "remover@email.com", "11912345678", NivelFuncionario.Pleno);

            _repository.Remove(vendedor);
            await _repository.SaveChangesAsync();

            var removido = await _repository.GetByIdAsync(vendedor.Id);
            removido.Should().BeNull();
        }

        // ==================== Método auxiliar ====================

        private async Task<Vendedor> SalvarVendedor(string nome, string email, string telefone, NivelFuncionario nivel)
        {
            var vendedor = new Vendedor(nome, email, telefone, "Senha123", 3000, nivel, DateTime.Now.AddDays(1));
            await _repository.AddAsync(vendedor);
            await _repository.SaveChangesAsync();
            return vendedor;
        }
    }
}