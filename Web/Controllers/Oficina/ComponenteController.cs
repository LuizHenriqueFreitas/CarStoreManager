using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComponenteController : ControllerBase
{
    private readonly IComponenteService _service;

    public ComponenteController(IComponenteService service)
    {
        _service = service;
    }

    // =========================
    // CONSULTAS
    // =========================

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

    [HttpGet("estoque-baixo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEstoqueBaixo()
    {
        var resultado = await _service.ObterComEstoqueBaixoAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("sistema/{sistema}")]
    public async Task<IActionResult> GetPorSistema(string sistema)
    {
        var resultado = await _service.ObterPorSistemaAsync(sistema);
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    // =========================
    // CRUD
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] CriarComponenteDTO dto)
    {
        var resultado = await _service.AddAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarComponenteDTO dto)
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

    // =========================
    // ESTOQUE
    // =========================

    [HttpPatch("{id:guid}/estoque/entrada")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EntradaEstoque(Guid id, [FromBody] MovimentacaoEstoqueDTO dto)
    {
        var resultado = await _service.EntradaEstoqueAsync(id, dto.Quantidade);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/estoque/saida")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> SaidaEstoque(Guid id, [FromBody] MovimentacaoEstoqueDTO dto)
    {
        var resultado = await _service.SaidaEstoqueAsync(id, dto.Quantidade);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }
}