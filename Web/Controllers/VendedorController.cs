using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendedorController : ControllerBase
{
    private readonly IVendedorService _service;

    public VendedorController(IVendedorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var resultado = await _service.ObterTodosAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.ObterPorIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarVendedorDTO dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVendedorDTO dto)
    {
        dto.Id = id;
        var resultado = await _service.AtualizarAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _service.RemoverAsync(id);
        return resultado.IsSuccess ? NoContent() : NotFound(resultado.Error);
    }
}