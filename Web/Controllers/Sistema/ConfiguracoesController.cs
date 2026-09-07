using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/configuracoes")]
[Authorize(Roles = "Admin")]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfiguracaoSistemaService _service;
    private readonly IExportacaoDadosService _exportacao;

    public ConfiguracoesController(IConfiguracaoSistemaService service, IExportacaoDadosService exportacao)
    {
        _service = service;
        _exportacao = exportacao;
    }

    /// <summary>Baixa todo o banco de dados da aplicação em um único arquivo JSON.</summary>
    [HttpGet("exportar-dados")]
    public async Task<IActionResult> ExportarDados(CancellationToken ct)
    {
        var r = await _exportacao.ExportarJsonAsync(ct);
        if (!r.IsSuccess) return BadRequest(r.Error);
        return File(r.Value!.Conteudo, "application/json", r.Value.NomeArquivo);
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var r = await _service.ObterAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] ConfiguracaoSistemaDTO dto)
    {
        var r = await _service.AtualizarAsync(dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpGet("margens")]
    public async Task<IActionResult> ObterMargens()
    {
        var r = await _service.ObterMargensAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPut("margens")]
    public async Task<IActionResult> AtualizarMargens([FromBody] MargensDTO dto)
    {
        var r = await _service.AtualizarMargensAsync(dto);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }
}
