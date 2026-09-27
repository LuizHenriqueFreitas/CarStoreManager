namespace CarStoreManager.Web.Components.Shared;

/// <summary>Sub-navegação da área Análises. Ver docs/redesign/28-relatorios-por-area.md.</summary>
public static class SubNavAnalises
{
    public static List<SubNav.Item> Itens => new()
    {
        new("/dashboard", "Visão geral"),
        new("/dashboard/relatorios", "Relatórios"),
    };
}
