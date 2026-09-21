using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Itens de sub-navegação da área Oficina — compartilhados entre o hub
/// (Gestão da Oficina), Recepção, Ordens de serviço, Estoque e Fornecedores.
/// Ver docs/redesign/02-tela-gestao-oficina.md §4.2.
///
/// "Fornecedores" só entra na lista se a matriz de permissões (doc 23)
/// liberar aquele papel pra essa página — antes disso, aparecia pra todo
/// não-mecânico (inclusive Recepcionista, que a própria rota já bloqueia).
/// </summary>
public static class SubNavOficina
{
    public static async Task<List<SubNav.Item>> ItensAsync(bool ehMecanico, RoleUsuario role, IPermissaoAcessoService permissaoService)
    {
        var lista = new List<SubNav.Item> { new("/oficina", "Visão geral") };
        if (!ehMecanico)
            lista.Add(new("/oficina/recepcao", "Recepção"));
        lista.Add(new("/oficina/ordens", "Ordens de serviço"));
        lista.Add(new("/oficina/componentes", "Estoque de peças"));
        if (!ehMecanico && await permissaoService.PodeAcessarAsync(role, "pagina:/oficina/fornecedores"))
            lista.Add(new("/oficina/fornecedores", "Fornecedores"));
        return lista;
    }
}
