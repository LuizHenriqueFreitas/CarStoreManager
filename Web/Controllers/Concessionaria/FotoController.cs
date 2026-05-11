// WebApi/Controllers/FotosController.cs
using Microsoft.AspNetCore.Mvc;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class FotosController : ControllerBase
{
    private readonly IFotoService _fotoService;

    public FotosController(IFotoService fotoService)
    {
        _fotoService = fotoService;
    }

    /// <summary>
    /// Upload de uma ou mais fotos para uma entidade (ex: VeiculoVenda, Cliente)
    /// </summary>
    [HttpPost("upload/{entidadeTipo}/{entidadeId:guid}")]
    public async Task<IActionResult> UploadFotos(string entidadeTipo, Guid entidadeId, IList<IFormFile> files)
    {
        var arquivos = (files ?? new List<IFormFile>())
            .Select(f => new ArquivoUpload(f.FileName, f.Length, f.ContentType, () => f.OpenReadStream()))
            .ToList();

        var result = await _fotoService.UploadFotosAsync(entidadeTipo, entidadeId, arquivos);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Lista todas as fotos de uma entidade
    /// </summary>
    [HttpGet("{entidadeTipo}/{entidadeId:guid}")]
    public async Task<IActionResult> GetFotos(string entidadeTipo, Guid entidadeId)
    {
        var result = await _fotoService.GetFotosByEntidadeAsync(entidadeTipo, entidadeId);
        if (!result.IsSuccess)
            return NotFound(result.Error);
        return Ok(result.Value);
    }

    /// <summary>
    /// Remove uma foto pelo ID
    /// </summary>
    [HttpDelete("{fotoId:guid}")]
    public async Task<IActionResult> RemoverFoto(Guid fotoId)
    {
        var result = await _fotoService.RemoverFotoAsync(fotoId);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return NoContent();
    }

    /// <summary>
    /// Reordena as fotos de uma entidade enviando a lista de IDs na ordem desejada
    /// </summary>
    [HttpPut("reordenar/{entidadeTipo}/{entidadeId:guid}")]
    public async Task<IActionResult> ReordenarFotos(string entidadeTipo, Guid entidadeId, [FromBody] List<Guid> ordemIds)
    {
        var result = await _fotoService.ReordenarFotosAsync(entidadeTipo, entidadeId, ordemIds);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok();
    }
}