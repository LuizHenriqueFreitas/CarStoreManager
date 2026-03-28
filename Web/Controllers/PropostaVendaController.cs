using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Vendedor")]
public class PropostaVendaController : ControllerBase
{
    private readonly IPropostaVendaService _service;

    public PropostaVendaController(IPropostaVendaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodas()
    {
        var resultado = await _service.ObterTodasAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.ObterPorIdAsync(id);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    [HttpGet("vendedor/{vendedorId:guid}")]
    public async Task<IActionResult> GetPorVendedor(Guid vendedorId)
    {
        var resultado = await _service.ObterPorVendedorAsync(vendedorId);
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("cliente/{clienteId:guid}")]
    public async Task<IActionResult> GetPorCliente(Guid clienteId)
    {
        var resultado = await _service.ObterPorClienteAsync(clienteId);
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPropostaVendaDTO dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/desconto")]
    public async Task<IActionResult> AplicarDesconto(Guid id, [FromBody] AplicarDescontoDTO dto)
    {
        dto.PropostaId = id;
        var resultado = await _service.AplicarDescontoAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/entrada")]
    public async Task<IActionResult> DefinirEntrada(Guid id, [FromBody] DefinirEntradaDTO dto)
    {
        dto.PropostaId = id;
        var resultado = await _service.DefinirEntradaAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/financiamento")]
    public async Task<IActionResult> GerarFinanciamento(Guid id, [FromBody] GerarFinanciamentoDTO dto)
    {
        dto.PropostaId = id;
        var resultado = await _service.GerarFinanciamentoAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        var resultado = await _service.AprovarAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/rejeitar")]
    public async Task<IActionResult> Rejeitar(Guid id)
    {
        var resultado = await _service.RejeitarAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var resultado = await _service.CancelarAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }
}