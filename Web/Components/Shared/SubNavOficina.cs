namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Itens de sub-navegação da área Oficina — compartilhados entre o hub
/// (Gestão da Oficina), Recepção, Ordens de serviço, Estoque e Fornecedores.
/// Ver docs/redesign/02-tela-gestao-oficina.md §4.2.
/// </summary>
public static class SubNavOficina
{
    public static List<SubNav.Item> Itens(bool ehMecanico) => ehMecanico
        ? new()
        {
            new("/oficina", "Visão geral"),
            new("/oficina/ordens", "Ordens de serviço"),
            new("/oficina/componentes", "Estoque de peças"),
        }
        : new()
        {
            new("/oficina", "Visão geral"),
            new("/oficina/recepcao", "Recepção"),
            new("/oficina/ordens", "Ordens de serviço"),
            new("/oficina/componentes", "Estoque de peças"),
            new("/oficina/fornecedores", "Fornecedores"),
        };
}
