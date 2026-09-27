using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Itens de sub-navegação da área Concessionária.
/// Ver docs/redesign/01-arquitetura-informacao.md §2.1.
/// </summary>
public static class SubNavConcessionaria
{
    // "Relatórios" aparece pra todo mundo que acessa a área (Admin,
    // GerenteVendas, Vendedor) — quem não tem acesso a nenhum relatório do
    // catálogo (só Admin/GerenteVendas veem os relatórios da concessionária,
    // ver RelatoriosGrid) simplesmente encontra o estado vazio lá, em vez de
    // um item sumindo do menu. "Documentos" é diferente: reúne termos e
    // contratos com dados pessoais do cliente, então só entra na lista pra
    // quem de fato tem acesso à Central de Documentos (doc 33) — sem estado
    // vazio pra Vendedor, o item nem aparece.
    public static List<SubNav.Item> Itens(RoleUsuario role)
    {
        var lista = new List<SubNav.Item>
        {
            new("/concessionaria", "Visão geral"),
            new("/concessionaria/salao", "Salão"),
            new("/concessionaria/propostas", "Propostas"),
            new("/concessionaria/test-drives", "Test drives"),
            new("/concessionaria/consignacoes", "Consignações"),
            new("/concessionaria/relatorios", "Relatórios"),
        };
        if (role is RoleUsuario.Admin or RoleUsuario.GerenteVendas)
            lista.Add(new("/concessionaria/documentos", "Documentos"));
        return lista;
    }

    // TODO(Épico F9): /concessionaria/test-drives depende da entidade TestDrive.
}
