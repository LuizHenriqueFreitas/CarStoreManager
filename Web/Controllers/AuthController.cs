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
}