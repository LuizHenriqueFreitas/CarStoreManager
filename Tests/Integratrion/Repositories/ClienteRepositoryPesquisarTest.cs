using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;

namespace CarStoreManager.Tests.Integration.Repositories;

/*
    Testes de regressão para o bug do PesquisarAsync que tinha
    "c.GetCpf().Contains(termo)" e estourava em runtime
    ("LINQ expression could not be translated").
*/
public class ClienteRepositoryPesquisarTest : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ClienteRepository _repo;

    // CPFs válidos pré-calculados (DV correto).
    private const string CpfValido1 = "11144477735";
    private const string CpfValido2 = "39053344705";
    private const string CpfValido3 = "52998224725";

    public ClienteRepositoryPesquisarTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repo = new ClienteRepository(_context);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task PesquisarAsync_TermoNoNome_RetornaCorrespondencias()
    {
        await SalvarCliente("João Silva", CpfValido1);
        await SalvarCliente("Maria Santos", CpfValido2);
        await SalvarCliente("Pedro Souza", CpfValido3);

        var resultado = await _repo.PesquisarAsync("Silva");

        resultado.Should().HaveCount(1);
        resultado[0].Nome.Should().Be("João Silva");
    }

    [Fact]
    public async Task PesquisarAsync_TermoEhCpfFormatado_RetornaCorrespondencia()
    {
        await SalvarCliente("João", CpfValido1);

        var resultado = await _repo.PesquisarAsync("111.444.777-35");

        resultado.Should().ContainSingle().Which.Nome.Should().Be("João");
    }

    [Fact]
    public async Task PesquisarAsync_TermoEhCpfSemFormatacao_RetornaCorrespondencia()
    {
        await SalvarCliente("João", CpfValido1);

        var resultado = await _repo.PesquisarAsync(CpfValido1);

        resultado.Should().ContainSingle();
    }

    [Fact]
    public async Task PesquisarAsync_TermoVazio_RetornaListaVazia()
    {
        await SalvarCliente("João", CpfValido1);

        var resultado = await _repo.PesquisarAsync("");

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task PesquisarAsync_SemMatch_RetornaListaVazia()
    {
        await SalvarCliente("João", CpfValido1);

        var resultado = await _repo.PesquisarAsync("XYZ");

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task PesquisarAsync_LimitaA20Resultados()
    {
        // Adiciona 21 clientes válidos com nomes que casam.
        // Como precisamos de 21 CPFs válidos diferentes e gerar é caro, repetimos com sufixo de email.
        var cpfs = new[] { CpfValido1, CpfValido2, CpfValido3 };
        for (var i = 0; i < 21; i++)
        {
            // Alterna entre CPFs válidos — repetir CPF não funcionaria por causa de unicidade implícita?
            // Aqui só persiste sem unicidade enforced, então tudo bem.
            var c = new Cliente($"ClienteX {i:D2}", $"c{i}@x.com", "11900000000", cpfs[i % 3]);
            await _repo.AddAsync(c);
        }
        await _repo.SaveChangesAsync();

        var resultado = await _repo.PesquisarAsync("ClienteX");

        resultado.Count.Should().Be(20);
    }

    private async Task SalvarCliente(string nome, string cpf)
    {
        var c = new Cliente(nome, $"{nome.Replace(" ", "").ToLower()}@x.com",
            "11900000000", cpf);
        await _repo.AddAsync(c);
        await _repo.SaveChangesAsync();
    }
}
