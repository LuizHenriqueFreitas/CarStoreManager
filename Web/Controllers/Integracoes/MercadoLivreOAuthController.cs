using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/mercadolivre")]
[Authorize(Roles = "Admin")]
public class MercadoLivreOAuthController : ControllerBase
{
    private readonly IMercadoLivreSincronizacaoService _service;

    public MercadoLivreOAuthController(IMercadoLivreSincronizacaoService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<IActionResult> ObterStatus()
    {
        var r = await _service.ObterConfiguracaoAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("conectar")]
    public async Task<IActionResult> Conectar()
    {
        var state = Guid.NewGuid().ToString("N");
        var r = await _service.ObterUrlConexaoAsync(state);
        return r.IsSuccess ? Ok(new { url = r.Value }) : BadRequest(r.Error);
    }

    /// <summary>
    /// Endpoint de retorno do OAuth — o navegador do admin é redirecionado para
    /// cá pelo próprio Mercado Livre após o consentimento. AllowAnonymous porque
    /// não há garantia de sessão nesse redirecionamento vindo de terceiro.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/mercadolivre/oauth/callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? state)
    {
        var r = await _service.ProcessarCallbackOAuthAsync(code);
        var destino = r.IsSuccess
            ? "/integracoes/mercadolivre?conectado=1"
            : $"/integracoes/mercadolivre?erro={Uri.EscapeDataString(r.Error ?? "erro desconhecido")}";
        return Redirect(destino);
    }

    [HttpPost("desconectar")]
    public async Task<IActionResult> Desconectar()
    {
        var r = await _service.DesconectarAsync();
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }
}
