using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.Interfaces.Sistema;

/// <summary>
/// Exporta os dados do sistema (Configurações → Exportar dados) no MESMO
/// formato aceito por <see cref="IImportacaoDadosService"/> — "chave" de
/// texto em vez de Guid, "cenario" reconstruindo o estágio de cada fluxo de
/// negócio — para que o arquivo baixado possa ser editado e reimportado
/// (Configurações → Importar dados) sem transformação manual.
/// </summary>
public interface IExportacaoDadosService
{
    Task<Result<ExportacaoDadosArquivo>> ExportarJsonAsync(CancellationToken ct = default);
}

/// <summary>Conteúdo pronto para download: nome sugerido + bytes do JSON (UTF-8).</summary>
public sealed record ExportacaoDadosArquivo(string NomeArquivo, byte[] Conteudo);
