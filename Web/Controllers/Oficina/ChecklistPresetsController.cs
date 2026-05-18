using CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;
using CarStoreManager.Application.Interfaces.Oficina;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers.Oficina;

[ApiController]
[Route("api/checklist-presets")]
public class ChecklistPresetsController : ControllerBase
{
    private readonly IChecklistPresetService _service;

    public ChecklistPresetsController(IChecklistPresetService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Listar()
    {
        var r = await _service.GetAllAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    /// <summary>
    /// Lookup leve para dropdowns — usado pela recepção ao criar OS para
    /// escolher qual preset aplicar.
    /// </summary>
    [HttpGet("lookup")]
    [Authorize(Roles = "Admin,Recepcionista,Mecanico")]
    public async Task<IActionResult> Lookup()
    {
        var r = await _service.GetLookupAtivosAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Recepcionista,Mecanico")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var r = await _service.GetByIdAsync(id);
        return r.IsSuccess ? Ok(r.Value) : NotFound(r.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] SalvarChecklistPresetDTO dto)
    {
        var r = await _service.AddAsync(dto);
        return r.IsSuccess ? Ok(new { id = r.Value }) : BadRequest(r.Error);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar([FromBody] SalvarChecklistPresetDTO dto)
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
