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
        var resultado = await _service.GetAllAsync();
        return resultado.IsSuccess ? Ok(resultado.Value) : BadRequest(resultado.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPorId(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
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
        var resultado = await _service.AddAsync(dto);
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

    [HttpPatch("{id:guid}/modo-pagamento")]
    public async Task<IActionResult> DefinirModoPagamento(Guid id, [FromBody] DefinirModoPagamentoDTO dto)
    {
        var resultado = await _service.DefinirModoPagamentoAsync(id, dto.ModoPagamento);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPost("{id:guid}/solicitar-financiamento")]
    public async Task<IActionResult> SolicitarFinanciamento(Guid id)
    {
        var resultado = await _service.SolicitarFinanciamentoAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPost("{id:guid}/resposta-financiadora")]
    public async Task<IActionResult> RegistrarRespostaFinanciadora(
        Guid id, [FromBody] RegistrarRespostaFinanciadoraDTO dto)
    {
        var resultado = await _service.RegistrarRespostaFinanciadoraAsync(id, dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        var resultado = await _service.AprovarAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/rejeitar")]
    public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarPropostaDTO dto)
    {
        var resultado = await _service.RejeitarAsync(id, dto.Motivo);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarPropostaDTO dto)
    {
        var resultado = await _service.CancelarAsync(id, dto.Motivo);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // ============================================================
    // Vistoria
    // ============================================================

    [HttpPost("{id:guid}/iniciar-vistoria")]
    public async Task<IActionResult> IniciarVistoria(Guid id)
    {
        var r = await _service.IniciarVistoriaAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPost("{id:guid}/vistoria")]
    public async Task<IActionResult> RegistrarVistoria(Guid id, [FromBody] RegistrarVistoriaDTO dto)
    {
        var vistoriadorId = ObterUsuarioId();
        var r = await _service.RegistrarVistoriaAsync(id, vistoriadorId, dto);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("{id:guid}/vistorias")]
    public async Task<IActionResult> ListarVistorias(Guid id)
    {
        var r = await _service.ListarVistoriasAsync(id);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    // ============================================================
    // Termo de entrega
    // ============================================================

    [HttpPut("{id:guid}/termo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarOuEditarTermo(Guid id, [FromBody] CriarOuEditarTermoDTO dto)
    {
        var adminId = ObterUsuarioId();
        var r = await _service.CriarOuEditarTermoAsync(id, adminId, dto);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("{id:guid}/termo")]
    public async Task<IActionResult> ObterTermo(Guid id)
    {
        var r = await _service.ObterTermoAsync(id);
        return r.IsSuccess ? Ok(r.Value) : NotFound(r.Error);
    }

    [HttpPost("{id:guid}/termo/enviar-assinatura")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnviarTermoParaAssinatura(Guid id)
    {
        var r = await _service.EnviarTermoParaAssinaturaAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    /// <summary>Endpoint público — cliente abre o link com token sem login.</summary>
    [AllowAnonymous]
    [HttpGet("/api/termo/{token}")]
    public async Task<IActionResult> ObterTermoPorToken(string token)
    {
        var r = await _service.ObterTermoPorTokenAsync(token);
        return r.IsSuccess ? Ok(r.Value) : NotFound(r.Error);
    }

    [AllowAnonymous]
    [HttpPost("/api/termo/{token}/assinar")]
    public async Task<IActionResult> AssinarTermo(string token, [FromBody] AssinarTermoDTO dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
        var r = await _service.AssinarTermoAsync(token, dto, ip);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    private Guid ObterUsuarioId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}