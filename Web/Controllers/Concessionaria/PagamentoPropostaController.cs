using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda.Pagamento;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers.Concessionaria;

[ApiController]
[Route("api/proposta/{propostaId:guid}/pagamentos")]
[Authorize(Roles = "Admin,Vendedor")]
public class PagamentoPropostaController : ControllerBase
{
    private readonly IPagamentoPropostaService _service;

    public PagamentoPropostaController(IPagamentoPropostaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar(Guid propostaId)
    {
        var r = await _service.ObterResumoAsync(propostaId);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(Guid propostaId, [FromBody] RegistrarPagamentoPropostaDTO dto)
    {
        var recebidoPor = ObterUsuarioId();
        var r = await _service.RegistrarPagamentoAsync(propostaId, recebidoPor, dto);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpDelete("/api/pagamento-proposta/{pagamentoId:guid}")]
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
