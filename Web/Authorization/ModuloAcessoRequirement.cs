using Microsoft.AspNetCore.Authorization;

namespace CarStoreManager.Web.Authorization;

/// <summary>
/// Requisito de autorização dinâmico: a rota só existe se o módulo de
/// negócio dono dela (Concessionária/Oficina) estiver ativo em
/// ConfiguracaoSistema — portão puro de liga/desliga, sem relação com
/// papel ou permissão individual. Nome da policy = "modulo:concessionaria"
/// ou "modulo:oficina", resolvida por RecursoPolicyProvider.
/// </summary>
public class ModuloAcessoRequirement : IAuthorizationRequirement
{
    public string Modulo { get; }

    public ModuloAcessoRequirement(string modulo) => Modulo = modulo;
}
