using CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/ordemservico/{ordemId:guid}/pagamentos")]
[Authorize(Roles = "Admin,Recepcionista")]
public class PagamentoOrdemServicoController : ControllerBase
{
    private readonly IPagamentoOrdemServicoService _service;

    public PagamentoOrdemServicoController(IPagamentoOrdemServicoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar(Guid ordemId)
    {
        var r = await _service.ObterResumoAsync(ordemId);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(Guid ordemId, [FromBody] RegistrarPagamentoDTO dto)
    {
        var recebidoPor = ObterUsuarioId();
        var r = await _service.RegistrarPagamentoAsync(ordemId, recebidoPor, dto);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpDelete("/api/pagamento/{pagamentoId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Estornar(Guid pagamentoId)
    {
        var r = await _service.EstornarPagamentoAsync(pagamentoId);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    private Guid ObterUsuarioId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
