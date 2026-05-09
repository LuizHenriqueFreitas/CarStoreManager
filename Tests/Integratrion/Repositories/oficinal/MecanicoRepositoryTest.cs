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
using CarStoreManager.Domain.Entities.Concessionaria;

namespace CarStoreManager.Tests.Integration.Repositories
{
    public class MecanicoRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly MecanicoRepository _repository;

        public MecanicoRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new MecanicoRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_MecanicoExistente_RetornaMecanico()
        {
            var mecanico = await SalvarMecanico("João", "joao@email.com", "11911111111", EspecialidadeMecanico.Mecanica);

            var resultado = await _repository.GetByIdAsync(mecanico.Id);

            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be("João");
            resultado.Email.Endereco.Should().Be("joao@email.com");
        }

        [Fact]
        public async Task GetByIdAsync_IdInexistente_RetornaNull()
        {
            var resultado = await _repository.GetByIdAsync(Guid.NewGuid());
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_TipoVendedor_NaoRetornaMecanico()
        {
            // Salva um Vendedor (não Mecanico) e garante que OfType<Mecanico> filtra
            var vendedor = new Vendedor("Vend", "vend@email.com", "11922222222", "Senha1", 2000, NivelFuncionario.Pleno, DateTime.UtcNow.AddDays(4));
            _context.Usuarios.Add(vendedor);
            await _context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(vendedor.Id);
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
        public async Task GetAllAsync_ComDados_RetornaApenasMecanicos()
        {
            var m1 = await SalvarMecanico("M1", "m1@email.com", "11911111111", EspecialidadeMecanico.Funilaria);
            var m2 = await SalvarMecanico("M2", "m2@email.com", "11922222222", EspecialidadeMecanico.Mecanica);
            // Adiciona um vendedor para garantir que não aparece
            var vendedor = new Vendedor("Vend", "vend@email.com", "11933333333", "hash", 2000, NivelFuncionario.Junior, DateTime.Today);
            _context.Usuarios.Add(vendedor);
            await _context.SaveChangesAsync();

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
            resultado.All(m => m.Role == RoleUsuario.Mecanico).Should().BeTrue();
        }

        // ==================== ObterPorEspecialidadeAsync ====================

        [Fact]
        public async Task ObterPorEspecialidadeAsync_EspecialidadeExistente_RetornaFiltrados()
        {
            await SalvarMecanico("M1", "m1@email.com", "11911111111", EspecialidadeMecanico.Mecanica);
            await SalvarMecanico("M2", "m2@email.com", "11922222222", EspecialidadeMecanico.Funilaria);
            await SalvarMecanico("M3", "m3@email.com", "11933333333", EspecialidadeMecanico.Mecanica);

            var resultado = await _repository.ObterPorEspecialidadeAsync(EspecialidadeMecanico.Mecanica);
            resultado.Should().HaveCount(2);
            resultado.All(m => m.Especialidade == EspecialidadeMecanico.Mecanica).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorEspecialidadeAsync_SemMecanicos_RetornaVazio()
        {
            await SalvarMecanico("M1", "m1@email.com", "11911111111", EspecialidadeMecanico.Funilaria);

            var resultado = await _repository.ObterPorEspecialidadeAsync(EspecialidadeMecanico.Pintura);
            resultado.Should().BeEmpty();
        }

        // ==================== ObterPorNivelAsync ====================

        [Fact]
        public async Task ObterPorNivelAsync_NivelExistente_RetornaFiltrados()
        {
            await SalvarMecanico("Junior", "j@email.com", "11911111111", EspecialidadeMecanico.Mecanica, NivelFuncionario.Junior);
            await SalvarMecanico("Pleno", "p@email.com", "11922222222", EspecialidadeMecanico.Funilaria, NivelFuncionario.Pleno);
            await SalvarMecanico("Pleno2", "p2@email.com", "11933333333", EspecialidadeMecanico.Eletrica, NivelFuncionario.Pleno);

            var resultado = await _repository.ObterPorNivelAsync(NivelFuncionario.Pleno);
            resultado.Should().HaveCount(2);
            resultado.All(m => m.DadosFuncionario.GetNivel() == NivelFuncionario.Pleno).Should().BeTrue();
        }

        [Fact]
        public async Task ObterPorNivelAsync_SemMecanicos_RetornaVazio()
        {
            await SalvarMecanico("Junior", "j@email.com", "11911111111", EspecialidadeMecanico.Pintura, NivelFuncionario.Junior);

            var resultado = await _repository.ObterPorNivelAsync(NivelFuncionario.Senior);
            resultado.Should().BeEmpty();
        }

        // ==================== ObterDisponiveisAsync ====================

        [Fact]
        public async Task ObterDisponiveisAsync_ComDisponiveis_RetornaApenasAtivosEDisponiveis()
        {
            var disponivel = await SalvarMecanico("Disp", "disp@email.com", "11911111111", EspecialidadeMecanico.Mecanica);
            disponivel.AlterarOcupado(); // por padrão, sem trabalhos, fica Disponivel

            var ocupado = await SalvarMecanico("Ocup", "ocup@email.com", "11922222222", EspecialidadeMecanico.Funilaria);
            // Força ocupado adicionando 3 trabalhos
            for (int i = 0; i < 3; i++) ocupado.TrabalhosAtivos.Add(Guid.NewGuid());
            ocupado.AlterarOcupado(); // Ocupado

            var inativo = await SalvarMecanico("Inativo", "inativo@email.com", "11933333333", EspecialidadeMecanico.Mecanica);
            inativo.Desativar(); // Ativo = false

            _context.SaveChanges();

            var resultado = await _repository.ObterDisponiveisAsync();
            resultado.Should().ContainSingle();
            resultado.First().Nome.Should().Be("Disp");
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_MecanicoValido_PersisteCorretamente()
        {
            var mecanico = new Mecanico("Carlos", "carlos@email.com", "11977777777", "hash", 2000,
                EspecialidadeMecanico.Eletrica, NivelFuncionario.Junior, DateTime.Today);
            await _repository.AddAsync(mecanico);
            await _repository.SaveChangesAsync();

            var salvo = await _repository.GetByIdAsync(mecanico.Id);
            salvo.Should().NotBeNull();
            salvo!.Nome.Should().Be("Carlos");
            salvo.Especialidade.Should().Be(EspecialidadeMecanico.Eletrica);
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_MecanicoExistente_AtualizaDados()
        {
            var mecanico = await SalvarMecanico("Antigo", "antigo@email.com", "11911111111", EspecialidadeMecanico.Mecanica);

            mecanico.AtualizarNome("Novo Nome");
            mecanico.AlterarEspecialidade(EspecialidadeMecanico.Eletrica);
            _repository.Update(mecanico);
            await _repository.SaveChangesAsync();

            var atualizado = await _repository.GetByIdAsync(mecanico.Id);
            atualizado!.Nome.Should().Be("Novo Nome");
            atualizado.Especialidade.Should().Be(EspecialidadeMecanico.Eletrica);
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_MecanicoExistente_RemoveRegistro()
        {
            var mecanico = await SalvarMecanico("Remover", "remover@email.com", "11912345678", EspecialidadeMecanico.Mecanica);

            _repository.Remove(mecanico);
            await _repository.SaveChangesAsync();

            var removido = await _repository.GetByIdAsync(mecanico.Id);
            removido.Should().BeNull();
        }

        // ==================== Método auxiliar ====================

        private async Task<Mecanico> SalvarMecanico(
            string nome,
            string email,
            string telefone,
            EspecialidadeMecanico especialidade,
            NivelFuncionario nivel = NivelFuncionario.Junior)
        {
            var mecanico = new Mecanico(nome, email, telefone, "hash", 3000, especialidade, nivel, DateTime.Today);
            await _repository.AddAsync(mecanico);
            await _repository.SaveChangesAsync();
            return mecanico;
        }
    }
}