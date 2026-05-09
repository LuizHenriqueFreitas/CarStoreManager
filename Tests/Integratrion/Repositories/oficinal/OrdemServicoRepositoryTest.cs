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
    public class OrdemServicoRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly OrdemServicoRepository _repository;

        public OrdemServicoRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new OrdemServicoRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_OrdemExistente_RetornaOrdemComItensEChecklist()
        {
            var ordem = await SalvarOrdemCompleta();

            var resultado = await _repository.GetByIdAsync(ordem.Id);

            resultado.Should().NotBeNull();
            resultado!.Descricao.Should().Be(ordem.Descricao);
            resultado.Itens.Should().NotBeNull();
            resultado.Checklist.Should().NotBeNull();
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
        public async Task GetAllAsync_ComDados_RetornaTodasComRelacionamentos()
        {
            await SalvarOrdemCompleta();
            await SalvarOrdemCompleta();

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(o =>
            {
                o.Itens.Should().NotBeNull();
                o.Checklist.Should().NotBeNull();
            });
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_OrdemValida_PersisteDados()
        {
            var ordem = CriarOrdemBasica();
            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();

            var salva = await _repository.GetByIdAsync(ordem.Id);
            salva.Should().NotBeNull();
            salva!.Descricao.Should().Be(ordem.Descricao);
            salva.NumeroPublico.Should().NotBeNullOrEmpty();
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_OrdemExistente_AtualizaDados()
        {
            var ordem = await SalvarOrdemBasica();
            var novaDescricao = "Descrição atualizada";

            // Atualiza via método de domínio
            ordem.GetType().GetProperty("Descricao")?.SetValue(ordem, novaDescricao);
            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            var atualizada = await _repository.GetByIdAsync(ordem.Id);
            atualizada!.Descricao.Should().Be(novaDescricao);
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_OrdemExistente_RemoveRegistro()
        {
            var ordem = await SalvarOrdemBasica();

            _repository.Remove(ordem);
            await _repository.SaveChangesAsync();

            var removida = await _repository.GetByIdAsync(ordem.Id);
            removida.Should().BeNull();
        }

        // ==================== ObterPorNumeroPublicoAsync ====================

        [Fact]
        public async Task ObterPorNumeroPublicoAsync_NumeroExistente_RetornaOrdemComChecklist()
        {
            var ordem = await SalvarOrdemCompleta();
            var numero = ordem.NumeroPublico;

            var resultado = await _repository.ObterPorNumeroPublicoAsync(numero);

            resultado.Should().NotBeNull();
            resultado!.NumeroPublico.Should().Be(numero);
            resultado.Checklist.Should().NotBeNull(); // Include
        }

        [Fact]
        public async Task ObterPorNumeroPublicoAsync_NumeroInexistente_RetornaNull()
        {
            var resultado = await _repository.ObterPorNumeroPublicoAsync("ABCDEFGH");
            resultado.Should().BeNull();
        }

        // ==================== ObterPorMecanicoAsync ====================

        [Fact]
        public async Task ObterPorMecanicoAsync_MecanicoComOrdens_RetornaFiltradasComItens()
        {
            var mecanicoId = Guid.NewGuid();
            await SalvarOrdemBasica(mecanicoId: mecanicoId);
            await SalvarOrdemBasica(mecanicoId: mecanicoId);
            await SalvarOrdemBasica(); // outro mecânico

            var resultado = await _repository.ObterPorMecanicoAsync(mecanicoId);
            resultado.Should().HaveCount(2);
            resultado.All(o => o.MecanicoId == mecanicoId).Should().BeTrue();
            resultado.Should().AllSatisfy(o => o.Itens.Should().NotBeNull());
        }

        [Fact]
        public async Task ObterPorMecanicoAsync_MecanicoSemOrdens_RetornaVazio()
        {
            var resultado = await _repository.ObterPorMecanicoAsync(Guid.NewGuid());
            resultado.Should().BeEmpty();
        }

        // ==================== ObterPorClienteAsync ====================

        [Fact]
        public async Task ObterPorClienteAsync_ClienteComOrdens_RetornaFiltradas()
        {
            var clienteId = Guid.NewGuid();
            await SalvarOrdemBasica(clienteId: clienteId);
            await SalvarOrdemBasica(clienteId: clienteId);
            await SalvarOrdemBasica(); // outro cliente

            var resultado = await _repository.ObterPorClienteAsync(clienteId);
            resultado.Should().HaveCount(2);
            resultado.All(o => o.ClienteId == clienteId).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorClienteAsync_ClienteSemOrdens_RetornaVazio()
        {
            var resultado = await _repository.ObterPorClienteAsync(Guid.NewGuid());
            resultado.Should().BeEmpty();
        }

        // ==================== ObterPorStatusAsync ====================

        [Fact]
        public async Task ObterPorStatusAsync_StatusExistente_RetornaFiltradas()
        {
            var pendente = await SalvarOrdemBasica(); // status Pendente
            var emAndamento = await SalvarOrdemBasica();
            emAndamento.Iniciar();
            await _context.SaveChangesAsync();

            var resultadoPendente = await _repository.ObterPorStatusAsync(StatusOrdemServico.Pendente);
            resultadoPendente.Should().ContainSingle().Which.Id.Should().Be(pendente.Id);

            var resultadoEmAndamento = await _repository.ObterPorStatusAsync(StatusOrdemServico.EmAndamento);
            resultadoEmAndamento.Should().ContainSingle().Which.Id.Should().Be(emAndamento.Id);
        }

        [Fact]
        public async Task ObterPorStatusAsync_StatusSemOrdens_RetornaVazio()
        {
            var resultado = await _repository.ObterPorStatusAsync(StatusOrdemServico.Finalizada);
            resultado.Should().BeEmpty();
        }

        // ==================== Integração com Itens ====================

        [Fact]
        public async Task GetByIdAsync_OrdemComItens_RetornaItensCorretamente()
        {
            var ordem = CriarOrdemBasica();
            var componenteId = Guid.NewGuid();
            ordem.AdicionarItem(new ItemOrdemServico(componenteId, ordem.Id, 3, 50.00m));
            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(ordem.Id);
            resultado!.Itens.Should().ContainSingle();
            resultado.Itens.First().Quantidade.Should().Be(3);
            resultado.Itens.First().ValorUnitario.GetValorDinheiro().Should().Be(50.00m);
        }

        [Fact]
        public async Task GetByIdAsync_OrdemComChecklist_RetornaChecklist()
        {
            var ordem = CriarOrdemBasica();
            ordem.AdicionarItemChecklist("Verificar óleo");
            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(ordem.Id);
            resultado!.Checklist.Should().ContainSingle();
            resultado.Checklist.First().Descricao.Should().Be("Verificar óleo");
        }

        // ==================== Métodos auxiliares ====================

        private OrdemServico CriarOrdemBasica(Guid? mecanicoId = null, Guid? clienteId = null)
        {
            return new OrdemServico(
                Guid.NewGuid(), // veiculoClienteId
                mecanicoId ?? Guid.NewGuid(),
                clienteId ?? Guid.NewGuid(),
                TipoServico.Manutencao,
                "Troca de óleo",
                DateTime.UtcNow.AddDays(5),
                200.00m
            );
        }

        private async Task<OrdemServico> SalvarOrdemBasica(Guid? mecanicoId = null, Guid? clienteId = null)
        {
            var ordem = CriarOrdemBasica(mecanicoId, clienteId);
            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();
            return ordem;
        }

        private async Task<OrdemServico> SalvarOrdemCompleta()
        {
            var ordem = CriarOrdemBasica();
            ordem.AdicionarItemChecklist("Item checklist");
            ordem.AdicionarItem(new ItemOrdemServico(Guid.NewGuid(), ordem.Id, 2, 100.00m));
            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();
            return ordem;
        }
    }
}