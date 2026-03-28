using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _service;

    public ClienteController(IClienteService service)
    {
        _service = service;
    }

    // =========================
    // CONSULTAS
    // =========================

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

    [HttpGet("cpf/{cpf}")]
    public async Task<IActionResult> GetPorCpf(string cpf)
    {
        var resultado = await _service.ObterPorCpfAsync(cpf);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarClienteDTO dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    // =========================
    // ATUALIZAÇÃO
    // =========================

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarClienteDTO dto)
    {
        dto.Id = id;
        var resultado = await _service.AtualizarAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // REMOÇÃO
    // =========================

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _service.RemoverAsync(id);
        return resultado.IsSuccess ? NoContent() : NotFound(resultado.Error);
    }
}