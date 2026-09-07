using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.Interfaces.Sistema;

/// <summary>
/// Exporta todo o banco de dados da aplicação para um único documento JSON
/// (Configurações → Exportar dados). É um retrato bruto das tabelas — pensado
/// para backup e inspeção, não para reimportação direta pelo importador de
/// demonstração.
/// </summary>
public interface IExportacaoDadosService
{
    Task<Result<ExportacaoDadosArquivo>> ExportarJsonAsync(CancellationToken ct = default);
}

/// <summary>Conteúdo pronto para download: nome sugerido + bytes do JSON (UTF-8).</summary>
public sealed record ExportacaoDadosArquivo(string NomeArquivo, byte[] Conteudo);
