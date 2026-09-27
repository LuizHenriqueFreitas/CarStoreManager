using CarStoreManager.Application.Services.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Web.Authorization;

/// <summary>
/// Gera policies em runtime a partir de CatalogoRecursosProtegiveis — sem
/// isso, precisaríamos pré-registrar 1 policy por página no Program.cs.
/// Nome da policy = a própria chave do catálogo ("pagina:/oficina/
/// fornecedores"); qualquer outro nome de policy (inclusive o padrão)
/// cai no provider padrão do ASP.NET Core.
/// </summary>
public class RecursoPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public RecursoPolicyProvider(IOptions<AuthorizationOptions> options) => _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (CatalogoRecursosProtegiveis.ObterPorChave(policyName) is not null)
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new RecursoAcessoRequirement(policyName))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        if (policyName is "modulo:concessionaria" or "modulo:oficina")
        {
            var modulo = policyName["modulo:".Length..];
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ModuloAcessoRequirement(modulo))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}
