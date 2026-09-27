namespace CarStoreManager.Web.Components.Shared;

/// <summary>Sub-navegação da área Clientes. Ver docs/redesign/28-relatorios-por-area.md.</summary>
public static class SubNavClientes
{
    public static List<SubNav.Item> Itens => new()
    {
        new("/clientes", "Visão geral"),
        new("/clientes/relatorios", "Relatórios"),
    };
}
