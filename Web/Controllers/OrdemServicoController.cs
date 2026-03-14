using Microsoft.AspNetCore.Mvc;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdemServicoController : ControllerBase
{
    private readonly IOrdemServicoService _ordemService;

    public OrdemServicoController(IOrdemServicoService ordemService)
    {
        _ordemService = ordemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodas()
    {
        var ordens = await _ordemService.ObterTodasAsync();
        return Ok(ordens);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(OrdemServicoDTO dto)
    {
        var ordem = await _ordemService.CriarAsync(dto);
        return Ok(ordem);
    }
}