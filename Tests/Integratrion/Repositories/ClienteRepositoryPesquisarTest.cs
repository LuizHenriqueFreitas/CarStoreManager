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
        // 21 clientes com CPFs distintos e válidos (CPF agora tem índice único).
        for (var i = 0; i < 21; i++)
        {
            var c = new Cliente($"ClienteX {i:D2}", $"c{i}@x.com", "11900000000", CpfValidoSeq(i),
                new CarStoreManager.Domain.Entities.Endereco("Rua A", "1", null, "Centro", "São Paulo", "SP", "01001000"));
            await _repo.AddAsync(c);
        }
        await _repo.SaveChangesAsync();

        var resultado = await _repo.PesquisarAsync("ClienteX");

        resultado.Count.Should().Be(20);
    }

    // Gera um CPF válido determinístico a partir de um índice (9 dígitos base + 2 DV).
    private static string CpfValidoSeq(int seed)
    {
        var b = (100000000 + seed * 7).ToString("D9").Select(ch => ch - '0').ToArray();
        int Dv(int[] n, int peso)
        {
            var s = 0;
            for (var i = 0; i < n.Length; i++) s += n[i] * (peso - i);
            var r = s % 11;
            return r < 2 ? 0 : 11 - r;
        }
        var d1 = Dv(b, 10);
        var com9 = b.Append(d1).ToArray();
        var d2 = Dv(com9, 11);
        return string.Concat(com9.Append(d2));
    }

    private async Task SalvarCliente(string nome, string cpf)
    {
        var c = new Cliente(nome, $"{nome.Replace(" ", "").ToLower()}@x.com",
            "11900000000", cpf,
            new CarStoreManager.Domain.Entities.Endereco("Rua A", "1", null, "Centro", "São Paulo", "SP", "01001000"));
        await _repo.AddAsync(c);
        await _repo.SaveChangesAsync();
    }
}
