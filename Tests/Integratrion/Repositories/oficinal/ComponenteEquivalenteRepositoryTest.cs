using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories;

public class ComponenteEquivalenteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ComponenteEquivalenteRepository _repository;

    public ComponenteEquivalenteRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new ComponenteEquivalenteRepository(_context);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task ObterPorComponenteAsync_ComoOriginal_RetornaLigacao()
    {
        var a = await SalvarComponente("A");
        var b = await SalvarComponente("B");
        await SalvarLigacao(a.Id, b.Id, TipoEquivalencia.Similar);

        var resultado = await _repository.ObterPorComponenteAsync(a.Id);

        resultado.Should().ContainSingle(l => l.ComponenteEquivalenteId == b.Id);
    }

    [Fact]
    public async Task ObterPorComponenteAsync_ComoRelacionado_RetornaLigacao()
    {
        var a = await SalvarComponente("A");
        var b = await SalvarComponente("B");
        await SalvarLigacao(a.Id, b.Id, TipoEquivalencia.Paralela);

        // Vínculo foi criado com A como Original — buscando pelo B (o
        // "outro lado" da FK) tem que encontrar o mesmo vínculo.
        var resultado = await _repository.ObterPorComponenteAsync(b.Id);

        resultado.Should().ContainSingle(l => l.ComponenteOriginalId == a.Id);
    }

    [Fact]
    public async Task ObterPorComponenteAsync_SemLigacoes_RetornaVazio()
    {
        var a = await SalvarComponente("A");

        var resultado = await _repository.ObterPorComponenteAsync(a.Id);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterLigacaoEntreAsync_ExisteNaOrdemInversa_RetornaLigacao()
    {
        var a = await SalvarComponente("A");
        var b = await SalvarComponente("B");
        await SalvarLigacao(a.Id, b.Id, TipoEquivalencia.Remanufaturada);

        var resultado = await _repository.ObterLigacaoEntreAsync(b.Id, a.Id);

        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterLigacaoEntreAsync_NaoExiste_RetornaNull()
    {
        var a = await SalvarComponente("A");
        var b = await SalvarComponente("B");

        var resultado = await _repository.ObterLigacaoEntreAsync(a.Id, b.Id);

        resultado.Should().BeNull();
    }

    private async Task<ComponenteEquivalente> SalvarLigacao(Guid originalId, Guid equivalenteId, TipoEquivalencia tipo)
    {
        var ligacao = new ComponenteEquivalente(originalId, equivalenteId, tipo);
        await _repository.AddAsync(ligacao);
        await _repository.SaveChangesAsync();
        return ligacao;
    }

    private async Task<Componente> SalvarComponente(string sku)
    {
        var c = new Componente(sku, $"Peça {sku}", "Descrição", "Marca", $"PN-{sku}",
            "", "", "87083010", "", "Freios", "UN", 0.5m, 180, Guid.NewGuid());
        await _context.Componentes.AddAsync(c);
        await _context.SaveChangesAsync();
        return c;
    }
}
