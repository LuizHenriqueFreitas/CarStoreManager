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
    public class VeiculoVendaRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _context;
        private readonly VeiculoVendaRepository _repository;

        public VeiculoVendaRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new VeiculoVendaRepository(_context);
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        // ==================== GetByIdAsync ====================

        [Fact]
        public async Task GetByIdAsync_VeiculoExistente_RetornaVeiculoComFotos()
        {
            var veiculo = await SalvarVeiculo("Honda", "Civic", "Preto", "2.0", 2022, 15000, "ABC1D23",
                TipoCambio.Automatico, TipoCombustivel.Flex, 85000.00m, AcessoriosVeiculo.ArCondicionado);

            var resultado = await _repository.GetByIdAsync(veiculo.Id);

            resultado.Should().NotBeNull();
            resultado!.Marca.Should().Be("Honda");
            resultado.Modelo.Should().Be("Civic");
            resultado.Fotos.Should().NotBeNull();
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
        public async Task GetAllAsync_ComDados_RetornaTodosComFotos()
        {
            var v1 = await SalvarVeiculo("Marca1", "Modelo1", "Cor1", "1.0", 2021, 10000, "AAA1111",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 50000.00m, AcessoriosVeiculo.Nenhum);
            var v2 = await SalvarVeiculo("Marca2", "Modelo2", "Cor2", "1.5", 2022, 20000, "BBB2222",
                TipoCambio.Automatico, TipoCombustivel.Flex, 75000.00m, AcessoriosVeiculo.VidrosEletricos);

            var resultado = await _repository.GetAllAsync();
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(v => v.Fotos.Should().NotBeNull());
        }

        // ==================== ObterDisponiveisAsync ====================

        [Fact]
        public async Task ObterDisponiveisAsync_FiltraApenasDisponiveis()
        {
            var disponivel1 = await SalvarVeiculo("Disp1", "M1", "Cor", "1.0", 2020, 1000, "AAA1111",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 30000.00m, AcessoriosVeiculo.Nenhum);
            var vendido = await SalvarVeiculo("Vendido", "M2", "Cor", "1.0", 2019, 5000, "BBB2222",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 25000.00m, AcessoriosVeiculo.Nenhum);
            vendido.MarcarComoVendido();
            await _context.SaveChangesAsync();

            var resultado = await _repository.ObterDisponiveisAsync();
            resultado.Should().ContainSingle();
            resultado.First().Id.Should().Be(disponivel1.Id);
        }

        // ==================== ObterPorDisponibilidadeAsync ====================

        [Fact]
        public async Task ObterPorDisponibilidadeAsync_FiltraPorStatus()
        {
            var disponivel = await SalvarVeiculo("Disp", "M", "Cor", "1.0", 2020, 1000, "CCC3333",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 40000.00m, AcessoriosVeiculo.Nenhum);
            var reservado = await SalvarVeiculo("Reservado", "M", "Cor", "1.0", 2020, 1000, "DDD4444",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 40000.00m, AcessoriosVeiculo.Nenhum);
            reservado.AlterarDisponibilidade(DisponibilidadeVeiculo.Reservado);
            await _context.SaveChangesAsync();

            var resultado = await _repository.ObterPorDisponibilidadeAsync(DisponibilidadeVeiculo.Reservado);
            resultado.Should().ContainSingle();
            resultado.First().Id.Should().Be(reservado.Id);
        }

        // ==================== ObterPorPlacaAsync ====================

        [Fact]
        public async Task ObterPorPlacaAsync_PlacaExistente_RetornaVeiculo()
        {
            var placa = "XYZ9876";
            var veiculo = await SalvarVeiculo("Toyota", "Corolla", "Branco", "2.0", 2021, 5000, placa,
                TipoCambio.Automatico, TipoCombustivel.Flex, 90000.00m, AcessoriosVeiculo.TetoSolar);

            var resultado = await _repository.ObterPorPlacaAsync(placa);
            resultado.Should().NotBeNull();
            Assert.Equal(placa, resultado!.GetPlacaCarro());
        }

        [Fact]
        public async Task ObterPorPlacaAsync_PlacaInexistente_RetornaNull()
        {
            var resultado = await _repository.ObterPorPlacaAsync("ZZZ0000");
            resultado.Should().BeNull();
        }

        // ==================== AddAsync ====================

        [Fact]
        public async Task AddAsync_VeiculoValido_PersisteDados()
        {
            var veiculo = new VeiculoVenda("Chevrolet", "Onix", "Prata", "1.4 Turbo",
                2023, 0, "ONX2023", "12345678900",
                TipoCambio.Automatico, TipoCombustivel.Flex, 72000.00m, AcessoriosVeiculo.BancoCouro);
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            var salvo = await _repository.GetByIdAsync(veiculo.Id);
            salvo.Should().NotBeNull();
            salvo!.Marca.Should().Be("Chevrolet");
            salvo.Modelo.Should().Be("Onix");
            salvo.Valor.GetValorDinheiro().Should().Be(72000.00m);
            salvo.GetAcessoriosVeiculo().Should().HaveFlag(AcessoriosVeiculo.BancoCouro);
        }

        // ==================== Update ====================

        [Fact]
        public async Task Update_VeiculoExistente_AtualizaDados()
        {
            var veiculo = await SalvarVeiculo("Ford", "Focus", "Azul", "2.0", 2020, 30000, "FCS1234",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 55000.00m, AcessoriosVeiculo.Nenhum);

            veiculo.AlterarMarca("Ford Updated");
            veiculo.MarcarComoVendido();
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();

            var atualizado = await _repository.GetByIdAsync(veiculo.Id);
            atualizado!.Marca.Should().Be("Ford Updated");
            atualizado.Disponibilidade.Should().Be(DisponibilidadeVeiculo.Vendido);
        }

        // ==================== Remove ====================

        [Fact]
        public async Task Remove_VeiculoExistente_RemoveRegistro()
        {
            var veiculo = await SalvarVeiculo("Remover", "Modelo", "Cor", "1.0", 2018, 50000, "REM0001",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 20000.00m, AcessoriosVeiculo.Nenhum);

            _repository.Remove(veiculo);
            await _repository.SaveChangesAsync();

            var removido = await _repository.GetByIdAsync(veiculo.Id);
            removido.Should().BeNull();
        }

        // ==================== Testes com fotos ====================

        [Fact]
        public async Task GetByIdAsync_VeiculoComFotos_RetornaFotosOrdenadas()
        {
            var veiculo = await SalvarVeiculo("Fotos", "F", "C", "1.0", 2020, 0, "FOT0001",
                TipoCambio.Manual, TipoCombustivel.Gasolina, 30000.00m, AcessoriosVeiculo.Nenhum);

            // Adiciona fotos diretamente no DbContext para que sejam reconhecidas como Added.
            _context.Fotos.Add(new Domain.Entities.Foto(veiculo.Id, "http://fotos.com/1.jpg", 0));
            _context.Fotos.Add(new Domain.Entities.Foto(veiculo.Id, "http://fotos.com/2.jpg", 1));
            await _context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(veiculo.Id);
            resultado!.Fotos.Should().HaveCount(2);
            resultado.Fotos.Select(f => f.Ordem).Should().Equal(0, 1);
        }

        // ==================== Método auxiliar ====================

        private async Task<VeiculoVenda> SalvarVeiculo(
            string marca, string modelo, string cor, string motorizacao,
            int ano, int quilometragem, string placa,
            TipoCambio cambio, TipoCombustivel combustivel,
            decimal valor, AcessoriosVeiculo acessorios)
        {
            var veiculo = new VeiculoVenda(marca, modelo, cor, motorizacao, ano, quilometragem, placa, "12345678900", cambio, combustivel, valor, acessorios);
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();
            return veiculo;
        }
    }
}