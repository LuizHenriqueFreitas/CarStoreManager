using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CarStoreManager.Web.Components.Shared.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _storage;
    private ClaimsPrincipal _anonimo = new(new ClaimsIdentity());

    public JwtAuthStateProvider(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var resultado = await _storage.GetAsync<string>("token");

            if (!resultado.Success || string.IsNullOrWhiteSpace(resultado.Value))
                return new AuthenticationState(_anonimo);

            var claims = ParsearToken(resultado.Value);
            if (claims is null)
                return new AuthenticationState(_anonimo);

            var identity = new ClaimsIdentity(claims, "jwt");
            var usuario = new ClaimsPrincipal(identity);

            return new AuthenticationState(usuario);
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
        var usuario = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(usuario)));
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

            if (jwt.ValidTo < DateTime.UtcNow)
                return null;

            return jwt.Claims;
        }
        catch
        {
            return null;
        }
    }
}