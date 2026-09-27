using System.Security.Claims;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CarStoreManager.Web.Authorization;

/// <summary>
/// Resolve RecursoAcessoRequirement consultando a mesma cadeia usada em
/// todo o resto do sistema (exceção individual > override de papel >
/// padrão do catálogo) — via IPermissaoAcessoService.PodeAcessarAsync.
/// Substitui, pras páginas do catálogo, o RolesAuthorizationRequirement
/// estático do ASP.NET Core, sem mudar o UX: falha aqui cai no mesmo
/// NotAuthorized que Routes.razor já tem pra [Authorize(Roles=...)].
/// </summary>
public class RecursoAcessoHandler : AuthorizationHandler<RecursoAcessoRequirement>
{
    private readonly IPermissaoAcessoService _permissaoService;

    public RecursoAcessoHandler(IPermissaoAcessoService permissaoService) => _permissaoService = permissaoService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RecursoAcessoRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return;

        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (!Enum.TryParse<RoleUsuario>(roleClaim, out var role)) return;

        var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(idClaim, out var usuarioId)) return;

        if (await _permissaoService.PodeAcessarAsync(usuarioId, role, requirement.RecursoChave))
            context.Succeed(requirement);
    }
}
