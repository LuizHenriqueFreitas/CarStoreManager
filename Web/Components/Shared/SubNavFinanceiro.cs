namespace CarStoreManager.Web.Components.Shared;

/// <summary>Sub-navegação da área Financeiro. Ver docs/redesign/03-tela-financeiro.md §3.</summary>
public static class SubNavFinanceiro
{
    public static List<SubNav.Item> Itens => new()
    {
        new("/financeiro", "Visão geral"),
        new("/financeiro/despesas", "Despesas"),
        new("/financeiro/a-receber", "A receber"),
        new("/financeiro/relatorios", "Relatórios"),
    };
}
