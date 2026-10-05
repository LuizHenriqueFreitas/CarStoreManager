using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.Interfaces.Sistema;

/// <summary>
/// Backup FIEL do sistema (Configurações → Backup completo): cópia exata do
/// arquivo SQLite em uso — senhas, históricos, status, valores, fotos
/// referenciadas, tudo. Diferente de <see cref="IExportacaoDadosService"/>,
/// que gera um roteiro JSON editável e é com perdas por projeto (ver
/// Geradores/dataset/PLANO_BASE_DEMO.md §8.2).
/// </summary>
public interface IBackupBancoService
{
    /// <summary>
    /// Cópia consistente do banco em uso (API de backup online do SQLite —
    /// funciona com o app rodando e em modo WAL). Nome sugerido:
    /// <c>delore-backup-AAAA-MM-DD-HHmm.db</c>.
    /// </summary>
    Task<Result<BackupBancoArquivo>> GerarBackupAsync(CancellationToken ct = default);

    /// <summary>
    /// Substitui TODO o conteúdo do banco em uso pelo arquivo enviado. Valida
    /// antes (SQLite íntegro, com <c>__EFMigrationsHistory</c>, sem migrations
    /// desconhecidas = versão mais nova que o app), grava uma cópia de
    /// segurança do banco atual na pasta de backups, restaura sem reiniciar o
    /// app e aplica as migrations pendentes.
    /// </summary>
    Task<Result<RestauracaoBackupResultado>> RestaurarAsync(Stream arquivo, CancellationToken ct = default);

    /// <summary>
    /// Backup automático (chamado ao subir o Web): se o banco existir e tiver
    /// dados, grava uma cópia na pasta de backups e mantém só as
    /// <paramref name="manter"/> mais recentes. Value = caminho gravado, ou
    /// null quando não havia nada a copiar.
    /// </summary>
    Task<Result<string?>> GerarBackupAutomaticoAsync(int manter = 7, CancellationToken ct = default);

    /// <summary>Pasta onde ficam os backups automáticos e as cópias de segurança.</summary>
    string PastaBackups { get; }
}

/// <summary>Arquivo de backup pronto para download.</summary>
public sealed record BackupBancoArquivo(string NomeArquivo, byte[] Conteudo);

/// <summary>Resultado de uma restauração bem-sucedida.</summary>
public sealed record RestauracaoBackupResultado(
    string CaminhoCopiaSeguranca,
    int MigrationsAplicadas);
