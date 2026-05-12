using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Authorize]
public class AlertaOSController : ControllerBase
{
    private readonly IAlertaOSService _service;

    public AlertaOSController(IAlertaOSService service) => _service = service;

    /// <summary>Mecânico emite alerta numa OS EmAndamento.</summary>
    [HttpPost("/api/ordemservico/{ordemId:guid}/alertas")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Emitir(Guid ordemId, [FromBody] CriarAlertaOSDTO dto)
    {
        var mecanicoId = ObterUsuarioId();
        var r = await _service.EmitirAsync(ordemId, mecanicoId, dto);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("/api/ordemservico/{ordemId:guid}/alertas")]
    public async Task<IActionResult> ListarPorOrdem(Guid ordemId)
    {
        var r = await _service.ListarPorOrdemAsync(ordemId);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    /// <summary>Fila da recepção com alertas pendentes do sistema.</summary>
    [HttpGet("/api/alertas-os/pendentes")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> ListarPendentes()
    {
        var r = await _service.ListarPendentesAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPost("/api/alertas-os/{id:guid}/resolver")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Resolver(Guid id, [FromBody] ResolverAlertaDTO dto)
    {
        var resolvidoPor = ObterUsuarioId();
        var r = await _service.ResolverAsync(id, resolvidoPor, dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    private Guid ObterUsuarioId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
