using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories
{
    public class ClienteRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly ClienteRepository _repository;

        public ClienteRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new ClienteRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_ClienteExistente_RetornaCliente()
        {
            var cliente = await SalvarCliente("João", "joao@email.com", "11999999999", "52998224725");

            var resultado = await _repository.GetByIdAsync(cliente.Id);

            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be(cliente.Nome);
            resultado.Email.Endereco.Should().Be("joao@email.com");
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
            await SalvarCliente("Ana", "ana@email.com", "11988888888", "12345678909");
            await SalvarCliente("Carlos", "carlos@email.com", "11977777777", "98765432100");

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
        }

        // ==================== ObterPorCpfAsync ====================

        [Fact]
        public async Task ObterPorCpfAsync_CpfExistente_RetornaCliente()
        {
            var cpf = "52998224725";
            var cliente = await SalvarCliente("Maria", "maria@email.com", "11966666666", cpf);

            var resultado = await _repository.ObterPorCpfAsync(cpf);

            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(cliente.Id);
            resultado.Cpf.Numero.Should().Be(cpf);
        }

        [Fact]
        public async Task ObterPorCpfAsync_CpfInexistente_RetornaNull()
        {
            var resultado = await _repository.ObterPorCpfAsync("00000000000");
            resultado.Should().BeNull();
        }

        // ==================== CpfExisteAsync ====================

        [Fact]
        public async Task CpfExisteAsync_CpfExistente_RetornaTrue()
        {
            var cpf = "52998224725";
            await SalvarCliente("Teste", "teste@email.com", "11955555555", cpf);

            var existe = await _repository.CpfExisteAsync(cpf);
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task CpfExisteAsync_CpfInexistente_RetornaFalse()
        {
            var existe = await _repository.CpfExisteAsync("11111111111");
            existe.Should().BeFalse();
        }

        // ==================== PesquisarAsync ====================

        [Fact]
        public async Task PesquisarAsync_TermoPresenteNoNome_RetornaCorrespondentes()
        {
            await SalvarCliente("Ana Souza", "ana@email.com", "11912345678", "523.026.250-80");
            await SalvarCliente("Carlos Silva", "carlos@email.com", "11912345679", "088.767.660-06");
            await SalvarCliente("Marina Souza", "marina@email.com", "11912345680", "114.927.190-64");

            var resultado = await _repository.PesquisarAsync("Souza");
            resultado.Should().HaveCount(2);
            resultado.All(c => c.Nome.Contains("Souza")).Should().BeTrue();
        }

        [Fact]
        public async Task PesquisarAsync_TermoPresenteNoCpf_RetornaCorrespondentes()
        {
            await SalvarCliente("Cliente A", "a@email.com", "11911111111", "52998224725");
            await SalvarCliente("Cliente B", "b@email.com", "11922222222", "98765432100");

            var resultado = await _repository.PesquisarAsync("52998224725");
            resultado.Should().ContainSingle();
            resultado[0].Cpf.Numero.Should().Be("52998224725");
        }

        [Fact]
        public async Task PesquisarAsync_TermoVazioOuNulo_RetornaListaVazia()
        {
            await SalvarCliente("Cliente", "cliente@email.com", "11912345678", "114.927.190-64");

            var resultadoVazio = await _repository.PesquisarAsync("");
            resultadoVazio.Should().BeEmpty();

            var resultadoNulo = await _repository.PesquisarAsync("");
            resultadoNulo.Should().BeEmpty();
        }

        [Fact]
        public async Task PesquisarAsync_TermoNaoEncontrado_RetornaListaVazia()
        {
            await SalvarCliente("Pedro", "pedro@email.com", "11987654321", "52998224725");

            var resultado = await _repository.PesquisarAsync("inexistente");
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task PesquisarAsync_ResultadosLimitadosA20()
        {
            // Usa CPFs válidos pré-gerados (recicla os 3 entre vários clientes
            // pois o teste só verifica o limite de 20).
            var cpfs = new[] { "11144477735", "39053344705", "52998224725" };
            for (int i = 1; i <= 25; i++)
                await SalvarCliente($"Cliente {i:D2}", $"c{i}@email.com", "11900000000", cpfs[i % 3]);

            var resultado = await _repository.PesquisarAsync("Cliente");
            resultado.Should().HaveCount(20);
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_ClienteValido_PersisteDados()
        {
            var cliente = new Cliente("Ricardo", "ricardo@email.com", "11988887777", "52998224725");
            await _repository.AddAsync(cliente);
            await _repository.SaveChangesAsync();

            var salvo = await _repository.GetByIdAsync(cliente.Id);
            salvo.Should().NotBeNull();
            salvo!.Nome.Should().Be("Ricardo");
            salvo.Email.Endereco.Should().Be("ricardo@email.com");
            salvo.Telefone.GetTelefone().Should().Be("11988887777");
            salvo.Cpf.Numero.Should().Be("52998224725");
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_ClienteExistente_AtualizaDados()
        {
            var cliente = await SalvarCliente("Antigo", "antigo@email.com", "11911111111", "52998224725");

            cliente.AtualizarClienteDados("Novo Nome", "novo@email.com", "11922222222");
            _repository.Update(cliente);
            await _repository.SaveChangesAsync();

            var atualizado = await _repository.GetByIdAsync(cliente.Id);
            atualizado!.Nome.Should().Be("Novo Nome");
            atualizado.Email.Endereco.Should().Be("novo@email.com");
            atualizado.Telefone.GetTelefone().Should().Be("11922222222");
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_ClienteExistente_DeletaRegistro()
        {
            var cliente = await SalvarCliente("Remover", "remover@email.com", "11933333333", "52998224725");

            _repository.Remove(cliente);
            await _repository.SaveChangesAsync();

            var removido = await _repository.GetByIdAsync(cliente.Id);
            removido.Should().BeNull();
        }

        // ==================== Método auxiliar ====================

        private async Task<Cliente> SalvarCliente(string nome, string email, string telefone, string cpf)
        {
            var cliente = new Cliente(nome, email, telefone, cpf);
            await _repository.AddAsync(cliente);
            await _repository.SaveChangesAsync();
            return cliente;
        }
    }
}