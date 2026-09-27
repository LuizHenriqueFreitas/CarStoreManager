using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.AspNetCore.Authorization;

namespace CarStoreManager.Web.Authorization;

/// <summary>
/// Resolve ModuloAcessoRequirement consultando
/// IConfiguracaoSistemaService.ObterModulosAtivosAsync — sem nenhuma
/// checagem de papel/usuário, é um portão mais grosso que fica acima de
/// tudo o resto (RecursoAcessoHandler, [Authorize(Roles=)]).
/// </summary>
public class ModuloAcessoHandler : AuthorizationHandler<ModuloAcessoRequirement>
{
    private readonly IConfiguracaoSistemaService _configuracaoService;

    public ModuloAcessoHandler(IConfiguracaoSistemaService configuracaoService) => _configuracaoService = configuracaoService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ModuloAcessoRequirement requirement)
    {
        var r = await _configuracaoService.ObterModulosAtivosAsync();
        if (!r.IsSuccess) return;

        var ativo = requirement.Modulo == "concessionaria" ? r.Value.Concessionaria : r.Value.Oficina;
        if (ativo)
            context.Succeed(requirement);
    }
}
