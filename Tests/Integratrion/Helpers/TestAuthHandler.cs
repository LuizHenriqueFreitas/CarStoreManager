using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Tests.Integratrion.Helpers;

/// <summary>
/// Handler de autenticação compartilhado entre todos os testes de integração de controllers.
/// O cliente envia <c>Authorization: Test &lt;Role&gt;</c> e o handler injeta uma identidade
/// autenticada com aquela role — assim os atributos [Authorize(Roles=...)] funcionam sem
/// precisar montar JWT real.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
#pragma warning disable CS0618 // ISystemClock é obsoleto mas o ASP.NET 9 ainda exige na base
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock) { }
#pragma warning restore CS0618

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Test "))
            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid header"));

        // Aceita múltiplas roles separadas por vírgula: "Test Admin,Vendedor"
        var rolesRaw = authHeader["Test ".Length..];
        var claims = new List<Claim> { new(ClaimTypes.Name, "TestUser") };
        foreach (var role in rolesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Açúcar para registrar o esquema de teste E reescrever a default policy
/// para usar esse esquema. Sem isso, controllers com <c>[Authorize]</c> de classe
/// caem na default policy do Program.cs (que usa o "Smart" scheme → Cookie → redireciona).
/// </summary>
public static class TestAuthExtensions
{
    public const string SchemeName = "Test";

    public static IServiceCollection AddTestAuth(this IServiceCollection services)
    {
        services.AddAuthentication(defaultScheme: SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(SchemeName, _ => { });

        // [Authorize] sem scheme cai aqui. Sem isso, ficaria preso na policy do Program.cs
        // que aponta para o "Smart" scheme e devolve HTML de login.
        services.PostConfigure<AuthorizationOptions>(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(SchemeName)
                .RequireAuthenticatedUser()
                .Build();
            options.FallbackPolicy = options.DefaultPolicy;
        });

        return services;
    }
}
