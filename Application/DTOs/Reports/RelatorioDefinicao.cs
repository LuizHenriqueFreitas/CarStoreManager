namespace CarStoreManager.Application.DTOs.Reports;

/// <summary>Agrupamento exibido na tela de Relatórios — ver docs/redesign/15-relatorios.md.</summary>
public enum GrupoRelatorio
{
    /// <summary>Retrato do estado atual: equipe, clientes, estoque, cadastros, transações.</summary>
    Operacional,

    /// <summary>Receitas, despesas, compras de material e vendas.</summary>
    Financeiro,

    /// <summary>Comparações de categorias (mesmo motor dos gráficos da Análises), exportadas em tabela.</summary>
    Comparativo
}

/// <summary>
/// Uma entrada do catálogo de relatórios — id estável + metadados pra
/// exibição na tela de Relatórios. É a única fonte da verdade do catálogo:
/// <see cref="Interfaces.IReportService.ObterCatalogo"/> monta a lista,
/// <see cref="Interfaces.IReportService.ExportAsync"/> consome o mesmo Id
/// pra montar e formatar o arquivo.
/// </summary>
public class RelatorioDefinicao
{
    public string Id { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public GrupoRelatorio Grupo { get; set; }
}
