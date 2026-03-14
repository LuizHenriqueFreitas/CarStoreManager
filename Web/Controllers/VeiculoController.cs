using Microsoft.AspNetCore.Mvc;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VeiculoController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculoController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        var veiculos = await _veiculoService.ObterTodosAsync();
        return Ok(veiculos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(VeiculoDTO dto)
    {
        var veiculo = await _veiculoService.CriarAsync(dto);
        return Ok(veiculo);
    }
}