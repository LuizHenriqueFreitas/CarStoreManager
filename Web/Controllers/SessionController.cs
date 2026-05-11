using System.Security.Claims;
using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

/*
    Endpoint que faz login server-side e EMITE COOKIE.
    O Cookie é o esquema padrão de autenticação para páginas Razor;
    sem ele, qualquer GET de página com [Authorize] retorna 401.

    Login.razor chama esse endpoint via fetch JS depois redireciona com forceLoad.
*/
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly IAuthService _authService;

    public SessionController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        if (!resultado.IsSuccess || resultado.Value is null)
            return Unauthorized(new { error = resultado.Error ?? "Credenciais inválidas" });

        // Decodifica claims do JWT pra criar a identidade do cookie.
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Value.Token);

        var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = resultado.Value.Expiracao
            });

        return Ok(new
        {
            token = resultado.Value.Token,
            nome = resultado.Value.Nome,
            role = resultado.Value.Role,
            expiracao = resultado.Value.Expiracao
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
