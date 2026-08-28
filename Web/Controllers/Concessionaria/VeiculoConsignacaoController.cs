using CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,GerenteVendas,Vendedor")]
public class VeiculoConsignacaoController : ControllerBase
{
    private readonly IVeiculoConsignacaoService _service;

    public VeiculoConsignacaoController(IVeiculoConsignacaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var resultado = await _service.GetAllAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("ativas")]
    public async Task<IActionResult> GetAtivas()
    {
        var resultado = await _service.ObterAtivasAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> Criar([FromBody] CriarVeiculoConsignacaoDTO dto)
    {
        var resultado = await _service.AddAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVeiculoConsignacaoDTO dto)
    {
        dto.Id = id;
        var resultado = await _service.UpdateAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _service.RemoveAsync(id);
        return resultado.IsSuccess ? NoContent() : NotFound(resultado.Error);
    }

    [HttpPatch("{id:guid}/renovar")]
    public async Task<IActionResult> Renovar(Guid id, [FromBody] int diasAdicionais = 90)
    {
        var resultado = await _service.RenovarAsync(id, diasAdicionais);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/vendida")]
    public async Task<IActionResult> MarcarComoVendida(Guid id)
    {
        var resultado = await _service.MarcarComoVendidaAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/concluir")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> ConcluirVenda(Guid id)
    {
        var resultado = await _service.ConcluirVendaAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/devolver")]
    public async Task<IActionResult> Devolver(Guid id)
    {
        var resultado = await _service.DevolverAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] string motivo)
    {
        var resultado = await _service.CancelarAsync(id, motivo);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }
}
