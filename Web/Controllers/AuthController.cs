using CarStoreManager.Application.DTOs.Auth;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        return resultado.IsSuccess ? Ok(resultado.Value) : Unauthorized(resultado.Error);
    }

    [HttpPost("usuarios")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDTO dto)
    {
        var resultado = await _authService.CriarUsuarioAsync(dto);
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    /// <summary>
    /// Reverifica a senha do usuário logado — usado em modais de confirmação
    /// antes de operações sensíveis (ex.: criar/editar veículo).
    /// 204 = senha confere; 401 = senha incorreta.
    /// </summary>
    [HttpPost("confirmar-senha")]
    [Authorize]
    public async Task<IActionResult> ConfirmarSenha([FromBody] ConfirmarSenhaDTO dto)
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(sub, out var usuarioId))
            return Unauthorized("Sessão inválida.");

        var r = await _authService.VerificarSenhaAsync(usuarioId, dto.Senha);
        return r.IsSuccess ? NoContent() : Unauthorized(r.Error);
    }
}