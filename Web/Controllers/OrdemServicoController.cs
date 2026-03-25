using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> GetTodas()
    {
        var resultado = await _service.ObterTodasAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.ObterPorIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    // Endpoint público — sem autenticação (AllowAnonymous será útil quando auth estiver pronto)
    [HttpGet("publica/{numeroPublico}")]
    public async Task<IActionResult> ConsultarPublica(string numeroPublico)
    {
        var resultado = await _service.ObterPorNumeroPublicoAsync(numeroPublico);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarOrdemServicoDTO dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    // =========================
    // ITENS
    // =========================

    [HttpPost("{id:guid}/itens")]
    public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemOrdemServicoDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AdicionarItemAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    public async Task<IActionResult> RemoverItem(Guid id, Guid itemId)
    {
        var resultado = await _service.RemoverItemAsync(id, itemId);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}/itens")]
    public async Task<IActionResult> AtualizarItem(Guid id, [FromBody] AtualizarItemOrdemServicoDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AtualizarItemAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // STATUS
    // =========================

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarOrdemServicoDTO dto)
    {
        dto.Id = id;
        var resultado = await _service.AtualizarStatusAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // CHECKLIST
    // =========================

    [HttpPost("{id:guid}/checklist")]
    public async Task<IActionResult> AdicionarItemChecklist(Guid id, [FromBody] AdicionarChecklistItemDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AdicionarItemChecklistAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/checklist/{itemId:guid}/status")]
    public async Task<IActionResult> AtualizarStatusChecklist(Guid id, Guid itemId, [FromBody] AtualizarStatusChecklistDTO dto)
    {
        dto.OrdemServicoId = id;
        dto.ItemId = itemId;
        var resultado = await _service.AtualizarStatusChecklistAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }
}