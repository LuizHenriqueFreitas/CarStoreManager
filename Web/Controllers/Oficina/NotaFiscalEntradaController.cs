using CarStoreManager.Application.DTOs.Oficina.NotaFiscal;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/nota-fiscal")]
[Authorize(Roles = "Admin")]
public class NotaFiscalEntradaController : ControllerBase
{
    private readonly INotaFiscalEntradaService _service;

    public NotaFiscalEntradaController(INotaFiscalEntradaService service)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var r = await _service.ListarAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id)
    {
        var r = await _service.ObterAsync(id);
        return r.IsSuccess ? Ok(r.Value) : NotFound(r.Error);
    }

    /// <summary>
    /// Importa o XML da NF-e. Aceita upload multipart (campo "arquivo") ou
    /// texto puro no body (Content-Type: application/xml).
    /// </summary>
    [HttpPost("importar")]
    [RequestSizeLimit(10_000_000)] // 10 MB — XML de NF-e raramente passa de 200 KB.
    public async Task<IActionResult> Importar()
    {
        string xml;

        if (Request.HasFormContentType && Request.Form.Files.Count > 0)
        {
            var file = Request.Form.Files[0];
            using var sr = new StreamReader(file.OpenReadStream());
            xml = await sr.ReadToEndAsync();
        }
        else
        {
            using var sr = new StreamReader(Request.Body);
            xml = await sr.ReadToEndAsync();
        }

        var r = await _service.ImportarXmlAsync(xml);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPatch("itens/{itemId:guid}/componente")]
    public async Task<IActionResult> VincularComponente(Guid itemId, [FromBody] VincularComponenteDTO dto)
    {
        var r = await _service.VincularComponenteAsync(itemId, dto.ComponenteId);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPatch("itens/{itemId:guid}/quantidade")]
    public async Task<IActionResult> AlterarQuantidade(Guid itemId, [FromBody] AlterarQuantidadeItemDTO dto)
    {
        var r = await _service.AlterarQuantidadeItemAsync(itemId, dto.Quantidade);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPost("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        var r = await _service.AprovarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPost("{id:guid}/rejeitar")]
    public async Task<IActionResult> Rejeitar(Guid id, [FromBody] RejeitarNotaDTO dto)
    {
        var r = await _service.RejeitarAsync(id, dto.Motivo);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    /// <summary>Sugestões automáticas de componentes para um item de NF-e.</summary>
    [HttpGet("itens/{itemId:guid}/sugestoes")]
    public async Task<IActionResult> SugerirComponentes(Guid itemId)
    {
        var r = await _service.SugerirComponentesAsync(itemId);
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    /// <summary>
    /// Cria componente a partir do item (campos vazios são preenchidos com
    /// dados do XML) e já vincula automaticamente.
    /// </summary>
    [HttpPost("itens/{itemId:guid}/cadastrar-e-vincular")]
    public async Task<IActionResult> CadastrarEVincular(
        Guid itemId,
        [FromBody] CarStoreManager.Application.DTOs.Oficina.Componente.CriarComponenteDTO dto)
    {
        var r = await _service.CriarComponenteEVincularAsync(itemId, dto);
        return r.IsSuccess ? Ok(new { ComponenteId = r.Value }) : BadRequest(r.Error);
    }
}
