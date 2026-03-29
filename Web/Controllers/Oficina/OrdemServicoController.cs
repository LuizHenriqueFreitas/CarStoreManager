using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdemServicoController : ControllerBase
{
    private readonly IOrdemServicoService _service;

    public OrdemServicoController(IOrdemServicoService service)
    {
        _service = service;
    }

    // =========================
    // CONSULTAS
    // =========================

    [HttpGet]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> GetTodas()
    {
        var resultado = await _service.GetAllAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    [HttpGet("publica/{numeroPublico}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublica(string numeroPublico)
    {
        var resultado = await _service.ObterPorNumeroPublicoAsync(numeroPublico);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Criar([FromBody] CriarOrdemServicoDTO dto)
    {
        var resultado = await _service.AddAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    // =========================
    // ITENS
    // =========================

    [HttpPost("{id:guid}/itens")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemOrdemServicoDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AdicionarItemAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> RemoverItem(Guid id, Guid itemId)
    {
        var resultado = await _service.RemoverItemAsync(id, itemId);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}/itens")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AtualizarItem(Guid id, [FromBody] AtualizarItemOrdemServicoDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AtualizarItemAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // STATUS
    // =========================

    [HttpPatch("{id:guid}/iniciar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Iniciar(Guid id)
    {
        var dto = new AtualizarOrdemServicoDTO { Id = id, Status = "EmAndamento" };
        var resultado = await _service.UpdateAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/finalizar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        var dto = new AtualizarOrdemServicoDTO { Id = id, Status = "Finalizada" };
        var resultado = await _service.UpdateAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var dto = new AtualizarOrdemServicoDTO { Id = id, Status = "Cancelada" };
        var resultado = await _service.UpdateAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // CHECKLIST
    // =========================

    [HttpPost("{id:guid}/checklist")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AdicionarItemChecklist(Guid id, [FromBody] AdicionarChecklistItemDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AdicionarItemChecklistAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/checklist/{itemId:guid}/status")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AtualizarStatusChecklist(
        Guid id, Guid itemId, [FromBody] AtualizarStatusChecklistDTO dto)
    {
        dto.OrdemServicoId = id;
        dto.ItemId = itemId;
        var resultado = await _service.AtualizarStatusChecklistAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // CÁLCULO
    // =========================

    [HttpPatch("{id:guid}/recalcular")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Recalcular(Guid id)
    {
        var resultado = await _service.RecalcularValoresAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }
}