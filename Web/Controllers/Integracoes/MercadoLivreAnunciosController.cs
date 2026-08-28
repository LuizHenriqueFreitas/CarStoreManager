using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Route("api/mercadolivre/anuncios")]
[Authorize(Roles = "Admin")]
public class MercadoLivreAnunciosController : ControllerBase
{
    private readonly IMercadoLivrePublicacaoService _publicacaoService;

    public MercadoLivreAnunciosController(IMercadoLivrePublicacaoService publicacaoService)
    {
        _publicacaoService = publicacaoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var r = await _publicacaoService.ListarAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Publicar([FromBody] PublicarAnuncioDTO dto)
    {
        var r = await _publicacaoService.PublicarAsync(dto);
        return r.IsSuccess ? Ok(new { itemIdML = r.Value }) : BadRequest(r.Error);
    }

    [HttpPost("{id:guid}/pausar")]
    public async Task<IActionResult> Pausar(Guid id)
    {
        var r = await _publicacaoService.PausarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }

    [HttpPost("{id:guid}/encerrar")]
    public async Task<IActionResult> Encerrar(Guid id)
    {
        var r = await _publicacaoService.EncerrarAsync(id);
        return r.IsSuccess ? NoContent() : BadRequest(r.Error);
    }
}
