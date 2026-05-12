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

    public ConfiguracoesController(IConfiguracaoSistemaService service) => _service = service;

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

    [HttpPost("testar-email")]
    public async Task<IActionResult> TestarEmail([FromBody] TestarEmailDTO dto)
    {
        var r = await _service.TestarEnvioAsync(dto.EmailDestino);
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
