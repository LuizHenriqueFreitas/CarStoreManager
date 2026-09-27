namespace CarStoreManager.Web.Components.Shared;

/// <summary>Sub-navegação da área Configurações (antiga "Cadastros").</summary>
public static class SubNavConfiguracoes
{
    public static List<SubNav.Item> Itens => new()
    {
        new("/configuracoes", "Sistema"),
    };
}
