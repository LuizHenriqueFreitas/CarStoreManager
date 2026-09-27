using Microsoft.AspNetCore.Authorization;

namespace CarStoreManager.Web.Authorization;

/// <summary>
/// Requisito de autorização dinâmico: acesso a um recurso do catálogo
/// (CatalogoRecursosProtegiveis), identificado pela própria chave
/// ("pagina:/oficina/fornecedores", etc.) — a chave também é o nome da
/// policy, resolvida em runtime por RecursoPolicyProvider.
/// </summary>
public class RecursoAcessoRequirement : IAuthorizationRequirement
{
    public string RecursoChave { get; }

    public RecursoAcessoRequirement(string recursoChave) => RecursoChave = recursoChave;
}
