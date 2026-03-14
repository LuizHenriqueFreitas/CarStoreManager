using Microsoft.AspNetCore.Mvc;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var clientes = await _clienteService.ObterTodosAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var cliente = await _clienteService.ObterPorIdAsync(id);

        if (cliente == null)
            return NotFound();

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(ClienteDTO dto)
    {
        var cliente = await _clienteService.CriarAsync(dto);
        return CreatedAtAction(nameof(GetPorId), cliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, ClienteDTO dto)
    {
        await _clienteService.AtualizarAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _clienteService.RemoverAsync(id);
        return NoContent();
    }
}