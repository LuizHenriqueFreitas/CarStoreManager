using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;

namespace CarStoreManager.Web.Components.Shared.Auth;

/*
    Provider híbrido:
      1) Se houver HttpContext (durante prerendering server-side OU em qualquer
         operação que tenha contexto HTTP ativo), usa o User do request — que
         já está autenticado pelo Cookie middleware. Isso garante que páginas
         Razor com [Authorize] passem no prerendering.
      2) Caso contrário (em circuit Blazor interativo), lê o JWT do
         ProtectedLocalStorage e materializa as claims a partir dele.
*/
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _storage;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipal _anonimo = new(new ClaimsIdentity());

    public JwtAuthStateProvider(
        ProtectedLocalStorage storage,
        IHttpContextAccessor httpContextAccessor)
    {
        _storage = storage;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 1) HttpContext disponível e usuário autenticado via cookie → usa.
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated ?? false)
            return new AuthenticationState(user);

        // 2) Sem HttpContext (estamos no circuit) → tenta o token salvo no browser.
        try
        {
            var resultado = await _storage.GetAsync<string>("token");
            if (!resultado.Success || string.IsNullOrWhiteSpace(resultado.Value))
                return new AuthenticationState(_anonimo);

            var claims = ParsearToken(resultado.Value);
            if (claims is null)
                return new AuthenticationState(_anonimo);

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(_anonimo);
        }
    }

    public async Task LoginAsync(string token)
    {
        await _storage.SetAsync("token", token);
        var claims = ParsearToken(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task LogoutAsync()
    {
        await _storage.DeleteAsync("token");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_anonimo)));
    }

    private static IEnumerable<Claim>? ParsearToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow) return null;
            return jwt.Claims;
        }
        catch
        {
            return null;
        }
    }
}
