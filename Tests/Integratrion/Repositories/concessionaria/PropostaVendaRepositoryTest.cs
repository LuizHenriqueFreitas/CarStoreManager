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
    public class PropostaVendaRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly PropostaVendaRepository _repository;

        public PropostaVendaRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new PropostaVendaRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_PropostaExistente_RetornaProposta()
        {
            var proposta = await SalvarProposta(valorBase: 100000m, desconto: 5);

            var resultado = await _repository.GetByIdAsync(proposta.Id);

            resultado.Should().NotBeNull();
            resultado!.ValorBase.GetValorDinheiro().Should().Be(100000m);
            resultado.ValorFinal.GetValorDinheiro().Should().Be(95000m);
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
        public async Task GetAllAsync_ComDados_RetornaTodas()
        {
            await SalvarProposta();
            await SalvarProposta();

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
        }

        // ==================== ObterPorVendedorAsync ====================

        [Fact]
        public async Task ObterPorVendedorAsync_VendedorComPropostas_RetornaFiltradas()
        {
            var vendedorId = Guid.NewGuid();
            await SalvarProposta(vendedorId: vendedorId);
            await SalvarProposta(vendedorId: vendedorId);
            await SalvarProposta(); // outro vendedor

            var resultado = await _repository.ObterPorVendedorAsync(vendedorId);
            resultado.Should().HaveCount(2);
            resultado.All(p => p.VendedorId == vendedorId).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorVendedorAsync_VendedorSemPropostas_RetornaVazio()
        {
            var resultado = await _repository.ObterPorVendedorAsync(Guid.NewGuid());
            resultado.Should().BeEmpty();
        }

        // ==================== ObterPorClienteAsync ====================

        [Fact]
        public async Task ObterPorClienteAsync_ClienteComPropostas_RetornaFiltradas()
        {
            var clienteId = Guid.NewGuid();
            await SalvarProposta(clienteId: clienteId);
            await SalvarProposta(clienteId: clienteId);
            await SalvarProposta(); // outro cliente

            var resultado = await _repository.ObterPorClienteAsync(clienteId);
            resultado.Should().HaveCount(2);
            resultado.All(p => p.ClienteId == clienteId).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorClienteAsync_ClienteSemPropostas_RetornaVazio()
        {
            var resultado = await _repository.ObterPorClienteAsync(Guid.NewGuid());
            resultado.Should().BeEmpty();
        }

        // ==================== ObterPorStatusAsync ====================

        [Fact]
        public async Task ObterPorStatusAsync_StatusExistente_RetornaFiltradas()
        {
            var aprovada = await SalvarProposta();
            aprovada.DefinirModoPagamento(ModoPagamento.Dinheiro);
            aprovada.Aprovar();
            await _context.SaveChangesAsync();

            var pendente = await SalvarProposta(); // Criada por padrão

            var resultadoAprovadas = await _repository.ObterPorStatusAsync(StatusPropostaVenda.Aprovada);
            resultadoAprovadas.Should().ContainSingle().Which.Id.Should().Be(aprovada.Id);

            var resultadoCriadas = await _repository.ObterPorStatusAsync(StatusPropostaVenda.Criada);
            resultadoCriadas.Should().ContainSingle().Which.Id.Should().Be(pendente.Id);
        }

        [Fact]
        public async Task ObterPorStatusAsync_StatusSemPropostas_RetornaVazio()
        {
            var resultado = await _repository.ObterPorStatusAsync(StatusPropostaVenda.Rejeitada);
            resultado.Should().BeEmpty();
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_PropostaValida_PersisteDados()
        {
            var proposta = new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50000m, 4m);
            await _repository.AddAsync(proposta);
            await _repository.SaveChangesAsync();

            var salva = await _repository.GetByIdAsync(proposta.Id);
            salva.Should().NotBeNull();
            salva!.ValorBase.GetValorDinheiro().Should().Be(50000m);
            salva.Desconto.GetDescontoValor().Should().Be(4m);
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_PropostaExistente_AtualizaDados()
        {
            var proposta = await SalvarProposta(valorBase: 80000m, desconto: 3m);

            proposta.AplicarDesconto(5m);
            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            var atualizada = await _repository.GetByIdAsync(proposta.Id);
            atualizada!.Desconto.GetDescontoValor().Should().Be(5m);
            atualizada.ValorFinal.GetValorDinheiro().Should().Be(76000m);
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_PropostaExistente_RemoveRegistro()
        {
            var proposta = await SalvarProposta();

            _repository.Remove(proposta);
            await _repository.SaveChangesAsync();

            var removida = await _repository.GetByIdAsync(proposta.Id);
            removida.Should().BeNull();
        }

        // ==================== Método auxiliar ====================

        private async Task<PropostaVenda> SalvarProposta(
            Guid? vendedorId = null,
            Guid? clienteId = null,
            decimal valorBase = 100000m,
            decimal desconto = 5m)
        {
            var proposta = new PropostaVenda(
                vendedorId ?? Guid.NewGuid(),
                Guid.NewGuid(), // veiculoId
                clienteId ?? Guid.NewGuid(),
                valorBase,
                desconto
            );
            await _repository.AddAsync(proposta);
            await _repository.SaveChangesAsync();
            return proposta;
        }
    }
}