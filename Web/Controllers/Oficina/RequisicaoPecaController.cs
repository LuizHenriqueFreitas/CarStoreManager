using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Authorize]
public class RequisicaoPecaController : ControllerBase
{
    private readonly IRequisicaoPecaService _service;

    public RequisicaoPecaController(IRequisicaoPecaService service) => _service = service;

    /// <summary>Mecânico abre requisição em uma OS específica.</summary>
    [HttpPost("/api/ordemservico/{ordemId:guid}/requisicoes")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Abrir(Guid ordemId, [FromBody] CriarRequisicaoPecaDTO dto)
    {
        var mecanicoId = ObterUsuarioId();
        var r = await _service.AbrirAsync(ordemId, mecanicoId, dto);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    /// <summary>Lista requisições de uma OS (qualquer perfil envolvido).</summary>
    [HttpGet("/api/ordemservico/{ordemId:guid}/requisicoes")]
    public async Task<IActionResult> ListarPorOrdem(Guid ordemId)
    {
        var r = await _service.ListarPorOrdemAsync(ordemId);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    /// <summary>Fila do admin com todas as pendentes do sistema.</summary>
    [HttpGet("/api/requisicoes-peca/pendentes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListarPendentes()
    {
        var r = await _service.ListarPendentesAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPost("/api/requisicoes-peca/{id:guid}/atender")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atender(Guid id, [FromBody] AtenderRequisicaoDTO dto)
    {
        var resolvidaPor = ObterUsuarioId();
        var r = await _service.AtenderAsync(id, resolvidaPor, dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPost("/api/requisicoes-peca/{id:guid}/rejeitar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarRequisicaoDTO dto)
    {
        var resolvidaPor = ObterUsuarioId();
        var r = await _service.RejeitarAsync(id, resolvidaPor, dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPost("/api/ordemservico/{ordemId:guid}/liberar-pos-requisicao")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LiberarOrdem(Guid ordemId)
    {
        var r = await _service.LiberarOrdemAsync(ordemId);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    private Guid ObterUsuarioId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
