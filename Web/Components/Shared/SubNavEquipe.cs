namespace CarStoreManager.Web.Components.Shared;

/// <summary>Sub-navegação da área Equipe. Ver docs/redesign/28-relatorios-por-area.md.</summary>
public static class SubNavEquipe
{
    public static List<SubNav.Item> Itens => new()
    {
        new("/equipe", "Visão geral"),
        new("/equipe/relatorios", "Relatórios"),
    };
}
