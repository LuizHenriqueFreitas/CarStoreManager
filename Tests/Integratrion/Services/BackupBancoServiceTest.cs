using System;
using System.IO;
using System.Linq;
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

/// <summary>
/// Backup fiel (Trilha C — Geradores/dataset/PLANO_BASE_DEMO.md §8.2): cópia
/// exata do SQLite em arquivo, restauração validada com cópia de segurança e
/// backup automático com rotação. Cada teste usa um banco em arquivo numa
/// pasta temporária própria (a API de backup do SQLite precisa de arquivo).
/// </summary>
public class BackupBancoServiceTests : IDisposable
{
    private readonly string _pasta;
    private readonly string _caminhoBanco;

    public BackupBancoServiceTests()
    {
        _pasta = Path.Combine(Path.GetTempPath(), "delore-backup-teste-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_pasta);
        _caminhoBanco = Path.Combine(_pasta, "carstore.db");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_pasta, recursive: true); } catch (IOException) { }
    }

    private static AppDbContext NovoContexto(string caminho) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={caminho}").Options);

    private static Cliente NovoCliente(string nome, string cpf) =>
        new(nome, $"{Guid.NewGuid():N}@email.com", "11988887777", cpf,
            new Endereco("Rua A", "1", null, "Centro", "São Paulo", "SP", "01001000"));

    private async Task<AppDbContext> CriarBancoComClienteAsync(string nome)
    {
        var ctx = NovoContexto(_caminhoBanco);
        await ctx.Database.MigrateAsync();
        ctx.Set<Cliente>().Add(NovoCliente(nome, "52998224725"));
        await ctx.SaveChangesAsync();
        return ctx;
    }

    private static async Task<string[]> NomesClientesAsync(string caminho)
    {
        await using var ctx = NovoContexto(caminho);
        return await ctx.Set<Cliente>().Select(c => c.Nome).OrderBy(n => n).ToArrayAsync();
    }

    private static BackupBancoService NovoServico(AppDbContext ctx) =>
        new(ctx, NullLogger<BackupBancoService>.Instance);

    [Fact]
    public async Task GerarBackupAsync_GeraArquivoSqliteValidoComOsMesmosDados()
    {
        await using var ctx = await CriarBancoComClienteAsync("Cliente Original");
        var servico = NovoServico(ctx);

        var r = await servico.GerarBackupAsync();

        r.IsSuccess.Should().BeTrue(r.Error);
        r.Value!.NomeArquivo.Should().MatchRegex(@"^delore-backup-\d{4}-\d{2}-\d{2}-\d{4}\.db$");
        r.Value.Conteudo.Take(15).Should().Equal("SQLite format 3"u8.ToArray());

        var copia = Path.Combine(_pasta, "copia.db");
        await File.WriteAllBytesAsync(copia, r.Value.Conteudo);
        (await NomesClientesAsync(copia)).Should().Equal("Cliente Original");

        await using var ctxCopia = NovoContexto(copia);
        (await ctxCopia.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task RestaurarAsync_SubstituiOConteudoECriaCopiaDeSegurancaAntes()
    {
        await using var ctx = await CriarBancoComClienteAsync("Cliente do Backup");
        var servico = NovoServico(ctx);
        var backup = (await servico.GerarBackupAsync()).Value!.Conteudo;

        // Estado "atual" diferente do backup.
        ctx.Set<Cliente>().RemoveRange(ctx.Set<Cliente>());
        ctx.Set<Cliente>().Add(NovoCliente("Cliente Posterior", "11144477735"));
        await ctx.SaveChangesAsync();

        var r = await servico.RestaurarAsync(new MemoryStream(backup));

        r.IsSuccess.Should().BeTrue(r.Error);
        (await NomesClientesAsync(_caminhoBanco)).Should().Equal("Cliente do Backup");

        // Cópia de segurança tem o estado de ANTES da restauração.
        r.Value!.CaminhoCopiaSeguranca.Should().StartWith(Path.Combine(_pasta, "backups"));
        File.Exists(r.Value.CaminhoCopiaSeguranca).Should().BeTrue();
        (await NomesClientesAsync(r.Value.CaminhoCopiaSeguranca)).Should().Equal("Cliente Posterior");

        // O próprio contexto do serviço enxerga o conteúdo restaurado.
        ctx.ChangeTracker.Clear();
        (await ctx.Set<Cliente>().Select(c => c.Nome).ToListAsync()).Should().Equal("Cliente do Backup");
    }

    [Fact]
    public async Task RestaurarAsync_ArquivoQueNaoESqlite_ERejeitadoSemAlterarOBanco()
    {
        await using var ctx = await CriarBancoComClienteAsync("Intocado");
        var servico = NovoServico(ctx);
        var lixo = new byte[4096];
        new Random(42).NextBytes(lixo);

        var r = await servico.RestaurarAsync(new MemoryStream(lixo));

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("SQLite");
        (await NomesClientesAsync(_caminhoBanco)).Should().Equal("Intocado");
        Directory.Exists(Path.Combine(_pasta, "backups")).Should().BeFalse("nada deve ser feito antes de validar o arquivo");
    }

    [Fact]
    public async Task RestaurarAsync_SqliteSemHistoricoDeMigrations_ERejeitado()
    {
        await using var ctx = await CriarBancoComClienteAsync("Intocado");
        var servico = NovoServico(ctx);

        var outro = Path.Combine(_pasta, "outro.db");
        await using (var c = new SqliteConnection($"Data Source={outro};Pooling=False"))
        {
            c.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = "CREATE TABLE Qualquer(Id INTEGER); INSERT INTO Qualquer VALUES (1);";
            cmd.ExecuteNonQuery();
        }

        await using var fs = File.OpenRead(outro);
        var r = await servico.RestaurarAsync(fs);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("não é um backup do DELORE");
        (await NomesClientesAsync(_caminhoBanco)).Should().Equal("Intocado");
    }

    [Fact]
    public async Task RestaurarAsync_BackupDeVersaoMaisNova_ERejeitado()
    {
        await using var ctx = await CriarBancoComClienteAsync("Intocado");
        var servico = NovoServico(ctx);

        var futuro = Path.Combine(_pasta, "futuro.db");
        await File.WriteAllBytesAsync(futuro, (await servico.GerarBackupAsync()).Value!.Conteudo);
        await using (var c = new SqliteConnection($"Data Source={futuro};Pooling=False"))
        {
            c.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('29991231000000_DoFuturo', '9.0.4');";
            cmd.ExecuteNonQuery();
        }

        await using var fs = File.OpenRead(futuro);
        var r = await servico.RestaurarAsync(fs);

        r.IsSuccess.Should().BeFalse();
        r.Error.Should().Contain("MAIS NOVA");
        (await NomesClientesAsync(_caminhoBanco)).Should().Equal("Intocado");
    }

    [Fact]
    public async Task GerarBackupAutomaticoAsync_GravaNaPastaBackupsEMantemOs7MaisRecentes()
    {
        await using var ctx = await CriarBancoComClienteAsync("Automático");
        var servico = NovoServico(ctx);

        var pasta = Path.Combine(_pasta, "backups");
        Directory.CreateDirectory(pasta);
        for (var dia = 1; dia <= 8; dia++)
            await File.WriteAllTextAsync(Path.Combine(pasta, $"auto-delore-backup-2020-01-{dia:00}-000000.db"), "antigo");
        var copiaSeguranca = Path.Combine(pasta, "pre-restauracao-2020-01-01-000000.db");
        await File.WriteAllTextAsync(copiaSeguranca, "nunca apagar");

        var r = await servico.GerarBackupAutomaticoAsync(manter: 7);

        r.IsSuccess.Should().BeTrue(r.Error);
        r.Value.Should().NotBeNull();
        servico.PastaBackups.Should().Be(pasta);
        (await NomesClientesAsync(r.Value!)).Should().Equal("Automático");

        var automaticos = Directory.GetFiles(pasta, "auto-delore-backup-*.db");
        automaticos.Should().HaveCount(7);
        automaticos.Should().Contain(r.Value);
        File.Exists(Path.Combine(pasta, "auto-delore-backup-2020-01-01-000000.db")).Should().BeFalse();
        File.Exists(Path.Combine(pasta, "auto-delore-backup-2020-01-02-000000.db")).Should().BeFalse();
        File.Exists(copiaSeguranca).Should().BeTrue("cópias de segurança de restauração não entram na rotação");
    }

    [Fact]
    public async Task GerarBackupAutomaticoAsync_BancoInexistente_NaoFazNada()
    {
        await using var ctx = NovoContexto(_caminhoBanco);
        var servico = NovoServico(ctx);

        var r = await servico.GerarBackupAutomaticoAsync();

        r.IsSuccess.Should().BeTrue();
        r.Value.Should().BeNull();
        File.Exists(_caminhoBanco).Should().BeFalse("o backup automático não pode criar o banco");
    }
}
