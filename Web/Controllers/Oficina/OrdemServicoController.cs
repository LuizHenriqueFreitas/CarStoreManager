using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Mecanico,Recepcionista")]
public class OrdemServicoController : ControllerBase
{
    private readonly IOrdemServicoService _service;

    public OrdemServicoController(IOrdemServicoService service)
    {
        _service = service;
    }

    // =========================
    // CONSULTAS
    // =========================

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

    [HttpGet("publica/{numeroPublico}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublica(string numeroPublico)
    {
        var resultado = await _service.ObterPorNumeroPublicoAsync(numeroPublico);
        return resultado.IsSuccess ? Ok(resultado.Value) : NotFound(resultado.Error);
    }

    // =========================
    // CRIAÇÃO — só admin/recepcionista (mecânico não abre OS)
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Criar([FromBody] CriarOrdemServicoDTO dto)
    {
        var resultado = await _service.AddAsync(dto);
        return resultado.IsSuccess
            ? CreatedAtAction(nameof(GetPorId), new { id = resultado.Value }, null)
            : BadRequest(resultado.Error);
    }

    // =========================
    // ITENS — admin e mecânico (recepcionista também, antes da revisão)
    // =========================

    [HttpPost("{id:guid}/itens")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista")]
    public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemOrdemServicoDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AdicionarItemAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista")]
    public async Task<IActionResult> RemoverItem(Guid id, Guid itemId)
    {
        var resultado = await _service.RemoverItemAsync(id, itemId);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}/itens")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista")]
    public async Task<IActionResult> AtualizarItem(Guid id, [FromBody] AtualizarItemOrdemServicoDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AtualizarItemAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // FLUXO DE APROVAÇÃO
    // =========================

    // recepcionista termina o orçamento e envia para o mecânico revisar
    [HttpPatch("{id:guid}/enviar-revisao")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> EnviarRevisao(Guid id)
    {
        var r = await _service.EnviarParaRevisaoAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // mecânico aprova o orçamento → vai para AguardandoCliente
    [HttpPatch("{id:guid}/aprovar-mecanico")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AprovarMecanico(Guid id)
    {
        var r = await _service.AprovarPeloMecanicoAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // mecânico devolve o orçamento pra recepcionista ajustar (volta a Pendente)
    [HttpPatch("{id:guid}/devolver")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Devolver(Guid id)
    {
        var r = await _service.DevolverParaAjustesAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // recepcionista marca que o cliente aprovou → Aprovada
    [HttpPatch("{id:guid}/cliente-aprovou")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> ClienteAprovou(Guid id)
    {
        var r = await _service.RegistrarAprovacaoDoClienteAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // mecânico inicia o serviço (a OS deve estar Aprovada)
    [HttpPatch("{id:guid}/iniciar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Iniciar(Guid id)
    {
        var r = await _service.IniciarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // mecânico finaliza o serviço técnico
    [HttpPatch("{id:guid}/finalizar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        var r = await _service.FinalizarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // recepção marca como entregue ao cliente (após cobrar)
    [HttpPatch("{id:guid}/entregar")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Entregar(Guid id)
    {
        var r = await _service.EntregarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var r = await _service.CancelarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    // =========================
    // CHECKLIST
    // =========================

    [HttpPost("{id:guid}/checklist")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista")]
    public async Task<IActionResult> AdicionarItemChecklist(Guid id, [FromBody] AdicionarChecklistItemDTO dto)
    {
        dto.OrdemServicoId = id;
        var resultado = await _service.AdicionarItemChecklistAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPatch("{id:guid}/checklist/{itemId:guid}/status")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AtualizarStatusChecklist(
        Guid id, Guid itemId, [FromBody] AtualizarStatusChecklistDTO dto)
    {
        dto.OrdemServicoId = id;
        dto.ItemId = itemId;
        var resultado = await _service.AtualizarStatusChecklistAsync(dto);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpPut("{id:guid}/checklist/{itemId:guid}")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> AtualizarDescricaoChecklist(
        Guid id, Guid itemId, [FromBody] AtualizarDescricaoChecklistDTO dto)
    {
        var resultado = await _service.AtualizarDescricaoChecklistAsync(id, itemId, dto.Descricao);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    [HttpDelete("{id:guid}/checklist/{itemId:guid}")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<IActionResult> RemoverItemChecklist(Guid id, Guid itemId)
    {
        var resultado = await _service.RemoverItemChecklistAsync(id, itemId);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }

    // =========================
    // CÁLCULO
    // =========================

    [HttpPatch("{id:guid}/recalcular")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Recalcular(Guid id)
    {
        var resultado = await _service.RecalcularValoresAsync(id);
        return resultado.IsSuccess ? NoContent() : BadRequest(resultado.Error);
    }
}
