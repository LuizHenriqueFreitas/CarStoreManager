namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Divide o catálogo único de relatórios (<see cref="CarStoreManager.Application.Interfaces.IReportService.ObterCatalogo"/>)
/// entre as 6 áreas que agora têm sua própria tela de Relatórios — ver
/// docs/redesign/28-relatorios-por-area.md. Cada Id aparece em exatamente
/// uma área (sem duplicação); os 2 relatórios "equipe-*" (rosters de
/// mecânicos/vendedores) ficam em Equipe, não em Oficina/Concessionária,
/// porque são sobre pessoas, não sobre a operação do setor.
/// </summary>
public static class CatalogoRelatoriosPorArea
{
    public static readonly HashSet<string> Concessionaria = new()
    {
        "estoque-veiculos-loja", "veiculos-consignados", "propostas-venda", "test-drives",
        "financeiro-concessionaria", "vendas-margem", "comparativo-concessionaria",
    };

    public static readonly HashSet<string> Oficina = new()
    {
        "estoque-pecas", "fornecedores", "ordens-servico",
        "financeiro-oficina", "compras-material", "comparativo-oficina", "comparativo-estoque",
    };

    public static readonly HashSet<string> Clientes = new() { "clientes" };

    public static readonly HashSet<string> Financeiro = new() { "financeiro-geral", "despesas-detalhadas" };

    public static readonly HashSet<string> Analises = new() { "comparativo-geral" };

    public static readonly HashSet<string> Equipe = new() { "equipe-oficina", "equipe-vendas" };
}
