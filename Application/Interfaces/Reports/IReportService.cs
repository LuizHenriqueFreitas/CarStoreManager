using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Reports;

namespace CarStoreManager.Application.Interfaces;

public interface IReportService
{
    /// <summary>Catálogo de relatórios visíveis pro usuário, já filtrado por papel.</summary>
    IReadOnlyList<RelatorioDefinicao> ObterCatalogo(bool podeOficina, bool podeConcessionaria);

    /// <summary>
    /// Monta e formata (CSV/XML) o relatório <paramref name="relatorioId"/>.
    /// Falha se o Id não existir no catálogo visível pra esse papel (mesma
    /// checagem de <see cref="ObterCatalogo"/>) — defesa contra acesso direto
    /// à URL por um usuário sem permissão pro setor daquele relatório.
    /// </summary>
    Task<Result<byte[]>> ExportAsync(
        string relatorioId, string formato, DateTime dataInicio, DateTime dataFim,
        bool podeOficina, bool podeConcessionaria);
}
