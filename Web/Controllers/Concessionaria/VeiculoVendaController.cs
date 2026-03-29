using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VeiculoVendaController : ControllerBase
{
    private readonly IVeiculoVendaService _service;

    public VeiculoVendaController(IVeiculoVendaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var resultado = await _service.GetAllAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("disponiveis")]
    public async Task<IActionResult> GetDisponiveis()
    {
        var resultado = await _service.ObterDisponiveisAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> Criar([FromBody] CriarVeiculoVendaDTO dto)
    {
        var resultado = await _service.AddAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVeiculoVendaDTO dto)
    {
        dto.Id = id;
        var resultado = await _service.UpdateAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _service.RemoveAsync(id);
        return resultado.IsSuccess ? NoContent() : NotFound(resultado.Error);
    }

    [HttpPatch("{id:guid}/vendido")]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> MarcarComoVendido(Guid id)
    {
        var resultado = await _service.MarcarComoVendidoAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/disponivel")]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> MarcarComoDisponivel(Guid id)
    {
        var resultado = await _service.MarcarComoDisponivelAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/quilometragem")]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> AtualizarQuilometragem(Guid id, [FromBody] int km)
    {
        var resultado = await _service.AtualizarQuilometragemAsync(id, km);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPost("{id:guid}/fotos")]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> AdicionarFoto(Guid id, [FromBody] AdicionarFotoDTO dto)
    {
        var resultado = await _service.AdicionarFotoAsync(id, dto.Url);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}/fotos/{fotoId:guid}")]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> RemoverFoto(Guid id, Guid fotoId)
    {
        var resultado = await _service.RemoverFotoAsync(id, fotoId);
        return resultado.IsSuccess ? NoContent() : NotFound(resultado.Error);
    }
}