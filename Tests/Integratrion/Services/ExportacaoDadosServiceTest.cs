using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Services.Sistema;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CarStoreManager.Tests.Integration.Services;

public class ExportacaoDadosServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ExportacaoDadosService _service;

    public ExportacaoDadosServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _service = new ExportacaoDadosService(_context, NullLogger<ExportacaoDadosService>.Instance);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task ExportarJsonAsync_BancoVazio_GeraJsonValidoComMeta()
    {
        var r = await _service.ExportarJsonAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value!.NomeArquivo.Should().EndWith(".json");

        using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(r.Value.Conteudo));
        doc.RootElement.TryGetProperty("_meta", out var meta).Should().BeTrue();
        meta.GetProperty("aplicacao").GetString().Should().Be("DELORE");
        doc.RootElement.TryGetProperty("Cliente", out var clientes).Should().BeTrue();
        clientes.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ExportarJsonAsync_ComRegistros_IncluiOsDados()
    {
        var cliente = new Cliente("Ricardo Exportado", "ricardo@email.com", "11988887777", "52998224725",
            new Endereco("Rua A", "1", null, "Centro", "São Paulo", "SP", "01001000"));
        _context.Set<Cliente>().Add(cliente);
        await _context.SaveChangesAsync();

        var r = await _service.ExportarJsonAsync();

        r.IsSuccess.Should().BeTrue();
        var json = Encoding.UTF8.GetString(r.Value!.Conteudo);
        json.Should().Contain("Ricardo Exportado");

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Cliente").GetArrayLength().Should().Be(1);
    }
}
