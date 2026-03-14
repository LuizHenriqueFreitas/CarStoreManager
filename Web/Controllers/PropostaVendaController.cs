using Microsoft.AspNetCore.Mvc;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropostaVendaController : ControllerBase
{
    private readonly IPropostaVendaService _propostaService;

    public PropostaVendaController(IPropostaVendaService propostaService)
    {
        _propostaService = propostaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodas()
    {
        var propostas = await _propostaService.ObterTodasAsync();
        return Ok(propostas);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(PropostaVendaDTO dto)
    {
        var proposta = await _propostaService.CriarAsync(dto);
        return Ok(proposta);
    }
}