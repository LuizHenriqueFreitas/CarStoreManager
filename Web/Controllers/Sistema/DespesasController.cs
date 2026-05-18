using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/despesas")]
[Authorize(Roles = "Admin")]
public class DespesasController : ControllerBase
{
    private readonly IDespesaService _service;

    public DespesasController(IDespesaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var r = await _service.GetAllAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var r = await _service.GetByIdAsync(id);
        return r.IsSuccess ? Ok(r.Value) : NotFound(r.Error);
    }

    [HttpGet("total-mensal")]
    public async Task<IActionResult> TotalMensal()
    {
        var r = await _service.ObterTotalMensalAsync();
        return r.IsSuccess ? Ok(new { total = r.Value }) : BadRequest(r.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarDespesaDTO dto)
    {
        var r = await _service.AddAsync(dto);
        return r.IsSuccess ? Ok(new { id = r.Value }) : BadRequest(r.Error);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarDespesaDTO dto)
    {
        var r = await _service.UpdateAsync(dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var r = await _service.RemoveAsync(id);
        return r.IsSuccess ? NoContent() : NotFound(r.Error);
    }
}
