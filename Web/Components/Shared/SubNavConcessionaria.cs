namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Itens de sub-navegação da área Concessionária.
/// Ver docs/redesign/01-arquitetura-informacao.md §2.1.
/// </summary>
public static class SubNavConcessionaria
{
    public static List<SubNav.Item> Itens => new()
    {
        new("/concessionaria", "Visão geral"),
        new("/concessionaria/salao", "Salão"),
        new("/concessionaria/propostas", "Propostas"),
        new("/concessionaria/test-drives", "Test drives"),
        new("/concessionaria/consignacoes", "Consignações"),
    };

    // TODO(Épico F9): /concessionaria/test-drives depende da entidade TestDrive.

}
