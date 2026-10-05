using System.Text;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Infrastructure.Services.Sistema;

/// <summary>
/// Backup/restauração fiel do banco SQLite em uso (Trilha C do
/// Geradores/dataset/PLANO_BASE_DEMO.md §8.2). Usa a API de backup online do
/// SQLite (<see cref="SqliteConnection.BackupDatabase(SqliteConnection)"/>),
/// que copia um retrato consistente mesmo com o app rodando e o banco em
/// modo WAL — diferente de copiar o arquivo .db na mão, que pode perder o
/// que ainda está só no -wal.
///
/// Todas as conexões abertas aqui são SEM pool (Pooling=False), pra não
/// deixar handle aberto em arquivo temporário nem segurar o banco principal.
/// </summary>
public sealed class BackupBancoService : IBackupBancoService
{
    private const string PrefixoAutomatico = "auto-delore-backup-";
    private const string PrefixoSeguranca = "pre-restauracao-";
    private static readonly byte[] CabecalhoSqlite = Encoding.ASCII.GetBytes("SQLite format 3\0");

    private readonly AppDbContext _db;
    private readonly ILogger<BackupBancoService> _logger;
    private readonly string? _caminhoBanco;

    public BackupBancoService(AppDbContext db, ILogger<BackupBancoService> logger)
    {
        _db = db;
        _logger = logger;
        _caminhoBanco = ResolverCaminhoBanco(db.Database.GetConnectionString());
    }

    public string PastaBackups =>
        Path.Combine(Path.GetDirectoryName(_caminhoBanco ?? Path.GetFullPath("carstore.db"))!, "backups");

    // =====================================================================
    // BAIXAR BACKUP
    // =====================================================================
    public async Task<Result<BackupBancoArquivo>> GerarBackupAsync(CancellationToken ct = default)
    {
        if (_caminhoBanco is null)
            return Result<BackupBancoArquivo>.Fail("O backup só está disponível quando o banco é um arquivo SQLite.");

        var temp = CaminhoTemporario("delore-backup");
        try
        {
            await Task.Run(() => CopiarBancoAtualPara(temp), ct);
            var bytes = await File.ReadAllBytesAsync(temp, ct);
            var nome = $"delore-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.db";
            return Result<BackupBancoArquivo>.Ok(new BackupBancoArquivo(nome, bytes));
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Falha ao gerar backup do banco.");
            return Result<BackupBancoArquivo>.Fail($"Não foi possível gerar o backup: {ex.Message}");
        }
        finally
        {
            ApagarComAuxiliares(temp);
        }
    }

    // =====================================================================
    // RESTAURAR BACKUP
    // =====================================================================
    public async Task<Result<RestauracaoBackupResultado>> RestaurarAsync(Stream arquivo, CancellationToken ct = default)
    {
        if (_caminhoBanco is null)
            return Result<RestauracaoBackupResultado>.Fail("A restauração só está disponível quando o banco é um arquivo SQLite.");

        var temp = CaminhoTemporario("delore-restaurar");
        try
        {
            await using (var fs = File.Create(temp))
                await arquivo.CopyToAsync(fs, ct);

            // 1) Valida o arquivo enviado ANTES de tocar no banco em uso.
            var validacao = await Task.Run(() => ValidarArquivoBackup(temp), ct);
            if (!validacao.IsSuccess)
                return Result<RestauracaoBackupResultado>.Fail(validacao.Error!);

            // 2) Cópia de segurança do banco atual — se falhar, não restaura.
            Directory.CreateDirectory(PastaBackups);
            var copiaSeguranca = Path.Combine(PastaBackups, $"{PrefixoSeguranca}{DateTime.Now:yyyy-MM-dd-HHmmss}.db");
            try
            {
                if (File.Exists(_caminhoBanco))
                    await Task.Run(() => CopiarBancoAtualPara(copiaSeguranca), ct);
            }
            catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
            {
                _logger.LogError(ex, "Falha ao gravar cópia de segurança antes de restaurar.");
                return Result<RestauracaoBackupResultado>.Fail(
                    $"Restauração cancelada: não foi possível salvar a cópia de segurança do banco atual ({ex.Message}).");
            }

            // 3) Restaura o conteúdo no banco em uso (sem reiniciar o app).
            try
            {
                await Task.Run(() => SobrescreverBancoAtualCom(temp), ct);
            }
            catch (SqliteException ex)
            {
                _logger.LogError(ex, "Falha ao restaurar backup.");
                return Result<RestauracaoBackupResultado>.Fail(
                    $"Não foi possível restaurar o backup ({ex.Message}). O banco atual não foi alterado; " +
                    $"cópia de segurança em {copiaSeguranca}.");
            }

            // 4) Migrations pendentes (backup de versão anterior do sistema).
            try
            {
                _db.ChangeTracker.Clear();
                var pendentes = (await _db.Database.GetPendingMigrationsAsync(ct)).Count();
                await _db.Database.MigrateAsync(ct);
                _logger.LogInformation("Backup restaurado ({Pendentes} migration(s) aplicada(s)). Cópia de segurança: {Copia}",
                    pendentes, copiaSeguranca);
                return Result<RestauracaoBackupResultado>.Ok(new RestauracaoBackupResultado(copiaSeguranca, pendentes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao aplicar migrations no backup restaurado — voltando a cópia de segurança.");
                try
                {
                    if (File.Exists(copiaSeguranca))
                        SobrescreverBancoAtualCom(copiaSeguranca);
                }
                catch (Exception exVolta)
                {
                    _logger.LogError(exVolta, "Falha ao voltar a cópia de segurança {Copia}.", copiaSeguranca);
                }
                return Result<RestauracaoBackupResultado>.Fail(
                    $"O backup foi carregado, mas a atualização de versão falhou ({ex.Message}). " +
                    $"O banco anterior foi reposto a partir da cópia de segurança ({copiaSeguranca}).");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Falha de E/S ao restaurar backup.");
            return Result<RestauracaoBackupResultado>.Fail($"Não foi possível ler o arquivo enviado: {ex.Message}");
        }
        finally
        {
            ApagarComAuxiliares(temp);
        }
    }

    // =====================================================================
    // BACKUP AUTOMÁTICO (ao subir o Web)
    // =====================================================================
    public async Task<Result<string?>> GerarBackupAutomaticoAsync(int manter = 7, CancellationToken ct = default)
    {
        if (_caminhoBanco is null || !File.Exists(_caminhoBanco) || new FileInfo(_caminhoBanco).Length == 0)
            return Result<string?>.Ok(null);

        try
        {
            if (!await Task.Run(BancoAtualTemDados, ct))
                return Result<string?>.Ok(null);

            Directory.CreateDirectory(PastaBackups);
            var destino = Path.Combine(PastaBackups, $"{PrefixoAutomatico}{DateTime.Now:yyyy-MM-dd-HHmmss}.db");
            await Task.Run(() => CopiarBancoAtualPara(destino), ct);

            // Rotação: só mexe nos automáticos (cópias de segurança de
            // restauração nunca são apagadas sozinhas).
            var antigos = Directory.GetFiles(PastaBackups, $"{PrefixoAutomatico}*.db")
                .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                .Skip(Math.Max(1, manter));
            foreach (var antigo in antigos)
                ApagarComAuxiliares(antigo);

            return Result<string?>.Ok(destino);
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Backup automático não foi gerado.");
            return Result<string?>.Fail($"Backup automático não foi gerado: {ex.Message}");
        }
    }

    // =====================================================================
    // AUXILIARES
    // =====================================================================
    private static string? ResolverCaminhoBanco(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return null;
        var fonte = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(fonte)
            || fonte.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
            || fonte.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            return null;
        return Path.GetFullPath(fonte);
    }

    private static SqliteConnection AbrirSemPool(string caminho, SqliteOpenMode modo = SqliteOpenMode.ReadWriteCreate)
    {
        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = caminho,
            Mode = modo,
            Pooling = false,
        }.ToString();
        var conexao = new SqliteConnection(cs);
        conexao.Open();
        return conexao;
    }

    private static string Pragma(SqliteConnection conexao, string sql)
    {
        using var cmd = conexao.CreateCommand();
        cmd.CommandText = sql;
        return Convert.ToString(cmd.ExecuteScalar()) ?? "";
    }

    /// <summary>Executa um PRAGMA sem esperar lock (até 2s); false se o banco estiver ocupado.</summary>
    private static bool TentarPragma(SqliteConnection conexao, string sql)
    {
        try
        {
            using var cmd = conexao.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 2;
            cmd.ExecuteScalar();
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6) // SQLITE_BUSY / SQLITE_LOCKED
        {
            return false;
        }
    }

    /// <summary>Retrato consistente do banco em uso → arquivo autossuficiente (sem -wal).</summary>
    private void CopiarBancoAtualPara(string destino)
    {
        ApagarComAuxiliares(destino);
        using var origem = AbrirSemPool(_caminhoBanco!, SqliteOpenMode.ReadWrite);
        using var dest = AbrirSemPool(destino);
        origem.BackupDatabase(dest);
        // O cabeçalho copiado diz "WAL" se a origem estiver em WAL; volta pra
        // DELETE pra que o arquivo .db sozinho tenha tudo.
        Pragma(dest, "PRAGMA journal_mode=DELETE;");
    }

    /// <summary>Copia o conteúdo de <paramref name="origemArquivo"/> para o banco em uso.</summary>
    private void SobrescreverBancoAtualCom(string origemArquivo)
    {
        // Fecha as conexões ociosas do pool (EF) — sem isso o journal_mode
        // não muda e conexões antigas ficam com o esquema em cache.
        SqliteConnection.ClearAllPools();

        using var origem = AbrirSemPool(origemArquivo, SqliteOpenMode.ReadWrite);
        using var dest = AbrirSemPool(_caminhoBanco!);
        var modoOriginal = Pragma(dest, "PRAGMA journal_mode;");
        // Em WAL o backup não aceita page_size diferente; em DELETE aceita.
        // Sair do WAL exige que ninguém mais tenha o banco aberto — se houver
        // outra conexão (outro processo, circuito no meio de uma consulta),
        // segue em WAL mesmo: com o page_size padrão igual, funciona.
        var saiuDoWal = TentarPragma(dest, "PRAGMA journal_mode=DELETE;");
        origem.BackupDatabase(dest);
        if (saiuDoWal && modoOriginal.Equals("wal", StringComparison.OrdinalIgnoreCase))
            TentarPragma(dest, "PRAGMA journal_mode=WAL;");

        SqliteConnection.ClearAllPools();
    }

    private Result ValidarArquivoBackup(string caminho)
    {
        var info = new FileInfo(caminho);
        if (info.Length < 512)
            return Result.Fail("O arquivo enviado está vazio ou não é um backup do DELORE (.db).");

        var cabecalho = new byte[CabecalhoSqlite.Length];
        using (var fs = File.OpenRead(caminho))
            fs.ReadExactly(cabecalho);
        if (!cabecalho.AsSpan().SequenceEqual(CabecalhoSqlite))
            return Result.Fail("O arquivo enviado não é um banco SQLite — escolha um arquivo .db gerado em \"Baixar backup\".");

        try
        {
            using var conexao = AbrirSemPool(caminho, SqliteOpenMode.ReadWrite);
            Pragma(conexao, "PRAGMA journal_mode=DELETE;");

            var check = Pragma(conexao, "PRAGMA quick_check;");
            if (!check.Equals("ok", StringComparison.OrdinalIgnoreCase))
                return Result.Fail($"O arquivo de backup está corrompido ({check}).");

            using (var cmd = conexao.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
                if (Convert.ToInt64(cmd.ExecuteScalar()) == 0)
                    return Result.Fail("O arquivo é um SQLite, mas não é um backup do DELORE (sem histórico de versões).");
            }

            var migrationsArquivo = new List<string>();
            using (var cmd = conexao.CreateCommand())
            {
                cmd.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory;";
                using var leitor = cmd.ExecuteReader();
                while (leitor.Read()) migrationsArquivo.Add(leitor.GetString(0));
            }

            if (migrationsArquivo.Count == 0)
                return Result.Fail("O arquivo não é um backup válido do DELORE (histórico de versões vazio).");

            var conhecidas = _db.Database.GetMigrations().ToHashSet(StringComparer.Ordinal);
            var desconhecidas = migrationsArquivo.Where(m => !conhecidas.Contains(m)).ToList();
            if (desconhecidas.Count > 0)
                return Result.Fail(
                    "Este backup é de uma versão MAIS NOVA do sistema do que a instalada " +
                    $"({desconhecidas.Count} atualização(ões) desconhecida(s), ex.: {desconhecidas[0]}). " +
                    "Atualize o sistema antes de restaurar.");

            return Result.Ok();
        }
        catch (SqliteException ex)
        {
            return Result.Fail($"O arquivo enviado não é um banco SQLite válido ({ex.Message}).");
        }
    }

    private bool BancoAtualTemDados()
    {
        using var conexao = AbrirSemPool(_caminhoBanco!, SqliteOpenMode.ReadWrite);
        var tabelas = new List<string>();
        using (var cmd = conexao.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' " +
                              "AND name NOT LIKE 'sqlite_%' AND name <> '__EFMigrationsHistory';";
            using var leitor = cmd.ExecuteReader();
            while (leitor.Read()) tabelas.Add(leitor.GetString(0));
        }

        foreach (var tabela in tabelas)
        {
            using var cmd = conexao.CreateCommand();
            cmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM \"{tabela.Replace("\"", "\"\"")}\");";
            if (Convert.ToInt64(cmd.ExecuteScalar()) == 1) return true;
        }
        return false;
    }

    private static string CaminhoTemporario(string prefixo) =>
        Path.Combine(Path.GetTempPath(), $"{prefixo}-{Guid.NewGuid():N}.db");

    private static void ApagarComAuxiliares(string caminho)
    {
        foreach (var arquivo in new[] { caminho, caminho + "-journal", caminho + "-wal", caminho + "-shm" })
        {
            try { if (File.Exists(arquivo)) File.Delete(arquivo); }
            catch (IOException) { /* temporário — o SO limpa depois */ }
            catch (UnauthorizedAccessException) { }
        }
    }
}
