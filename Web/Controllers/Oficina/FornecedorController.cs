using CarStoreManager.Application.DTOs.Oficina.Fornecedor;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ChefeOficina,Mecanico")]
public class FornecedorController : ControllerBase
{
    private readonly IFornecedorService _service;

    public FornecedorController(IFornecedorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var resultado = await _service.GetAllAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> Criar([FromBody] CriarFornecedorDTO dto)
    {
        var resultado = await _service.AddAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarFornecedorDTO dto)
    {
        dto.Id = id;
        var resultado = await _service.UpdateAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/ativar")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> Ativar(Guid id)
    {
        var resultado = await _service.AtivarAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/desativar")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var resultado = await _service.DesativarAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _service.RemoveAsync(id);
        return resultado.IsSuccess ? NoContent() : NotFound(resultado.Error);
    }
}
