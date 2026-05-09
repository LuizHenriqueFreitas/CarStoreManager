using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MecanicoController : ControllerBase
{
    private readonly IMecanicoService _service;

    public MecanicoController(IMecanicoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var result = await _service.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("disponiveis")]
    public async Task<IActionResult> GetDisponiveis()
    {
        var result = await _service.ObterDisponiveisAsync();
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarMecanicoDTO dto)
    {
        var result = await _service.AddAsync(dto);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = result.Value }, null)
            : BadRequest(result.Error);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar(AtualizarMecanicoDTO dto)
    {
        var result = await _service.UpdateAsync(dto);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var result = await _service.RemoveAsync(id);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}