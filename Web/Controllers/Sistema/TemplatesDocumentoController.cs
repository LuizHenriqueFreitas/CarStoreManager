using CarStoreManager.Application.DTOs.Sistema.TemplateDocumento;
using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers.Sistema;

/// <summary>
/// CRUD dos templates de documento/contrato (termo de entrega, contrato de
/// consignação, resposta de financiadora, etc.) — mesmo padrão do
/// ChecklistPresetsController: só o admin gerencia os presets, mas qualquer
/// papel que redige um contrato precisa ler o lookup pra escolher um.
/// </summary>
[ApiController]
[Route("api/templates-documento")]
public class TemplatesDocumentoController : ControllerBase
{
    private readonly ITemplateDocumentoService _service;

    public TemplatesDocumentoController(ITemplateDocumentoService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Listar()
    {
        var r = await _service.GetAllAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    /// <summary>Lookup leve para os seletores de template espalhados pelo sistema.</summary>
    [HttpGet("lookup")]
    [Authorize(Roles = "Admin,GerenteVendas,Vendedor")]
    public async Task<IActionResult> Lookup()
    {
        var r = await _service.GetLookupAtivosAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,GerenteVendas,Vendedor")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var r = await _service.GetByIdAsync(id);
        return r.IsSuccess ? Ok(r.Value) : NotFound(r.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] SalvarTemplateDocumentoDTO dto)
    {
        var r = await _service.AddAsync(dto);
        return r.IsSuccess ? Ok(new { id = r.Value }) : BadRequest(r.Error);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar([FromBody] SalvarTemplateDocumentoDTO dto)
    {
        var r = await _service.UpdateAsync(dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var r = await _service.RemoveAsync(id);
        return r.IsSuccess ? NoContent() : NotFound(r.Error);
    }
}
