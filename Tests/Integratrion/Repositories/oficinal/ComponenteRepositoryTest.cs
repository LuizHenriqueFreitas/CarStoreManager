using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories;

public class ComponenteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ComponenteRepository _repository;

    public ComponenteRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
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

    [Fact]
    public async Task GetByIdAsync_ComponenteExistente_RetornaComponente()
    {
        var c = await SalvarComponente(sku: "F-01", nome: "Filtro de óleo");

        var resultado = await _repository.GetByIdAsync(c.Id);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Filtro de óleo");
        resultado.SKUInterno.Should().Be("F-01");
    }

    [Fact]
    public async Task GetByIdAsync_IdInexistente_RetornaNull()
    {
        var resultado = await _repository.GetByIdAsync(Guid.NewGuid());
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_SemDados_RetornaListaVazia()
    {
        var resultado = await _repository.GetAllAsync();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ComDados_RetornaTodos()
    {
        await SalvarComponente(sku: "P-100", nome: "Pastilha", partNumber: "PN-100");
        await SalvarComponente(sku: "O-20", nome: "Óleo", partNumber: "PN-200");

        var resultado = await _repository.GetAllAsync();
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAsync_PersisteComponente()
    {
        var c = CriarComponente();
        await _repository.AddAsync(c);
        await _repository.SaveChangesAsync();

        var doBanco = await _context.Componentes.FindAsync(c.Id);
        doBanco.Should().NotBeNull();
        doBanco!.NCM.Should().Be("87083010");
    }

    [Fact]
    public async Task Remove_ApagaDoBanco()
    {
        var c = await SalvarComponente();
        _repository.Remove(c);
        await _repository.SaveChangesAsync();

        (await _context.Componentes.FindAsync(c.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Update_PersisteAlteracoes()
    {
        var c = await SalvarComponente();
        c.SetNome("Nome alterado");
        _repository.Update(c);
        await _repository.SaveChangesAsync();

        var doBanco = await _context.Componentes.FindAsync(c.Id);
        doBanco!.Nome.Should().Be("Nome alterado");
    }

    private async Task<Componente> SalvarComponente(
        string sku = "PFD-001",
        string nome = "Pastilha de freio",
        string partNumber = "PN-12345")
    {
        var c = CriarComponente(sku: sku, nome: nome, partNumber: partNumber);
        await _repository.AddAsync(c);
        await _repository.SaveChangesAsync();
        return c;
    }

    private static Componente CriarComponente(
        string sku = "PFD-001",
        string nome = "Pastilha de freio",
        string partNumber = "PN-12345")
        => new(
            sku, nome, "Descrição", "Bosch", partNumber, "OEM-1", "7891234567890",
            "87083010", "0102000", "Freios", "UN", 0.5m, 180);
}
