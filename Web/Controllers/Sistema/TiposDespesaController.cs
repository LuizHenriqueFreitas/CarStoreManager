using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/tipos-despesa")]
[Authorize(Roles = "Admin")]
public class TiposDespesaController : ControllerBase
{
    private readonly ITipoDespesaService _service;

    public TiposDespesaController(ITipoDespesaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var r = await _service.GetAllAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("ativos")]
    public async Task<IActionResult> ListarAtivos()
    {
        var r = await _service.GetAtivosAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] SalvarTipoDespesaDTO dto)
    {
        var r = await _service.AddAsync(dto);
        return r.IsSuccess ? Ok(new { id = r.Value }) : BadRequest(r.Error);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] SalvarTipoDespesaDTO dto)
    {
        var r = await _service.UpdateAsync(dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var r = await _service.RemoveAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }
}
