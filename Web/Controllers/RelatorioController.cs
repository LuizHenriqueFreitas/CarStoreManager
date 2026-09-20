using CarStoreManager.Application.DTOs.Reports;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

/*
    Catálogo único de relatórios — ver docs/redesign/15-relatorios.md e
    ReportService.cs (onde cada Id do catálogo tem seu builder). Todo
    relatório aceita um intervalo [dataInicio, dataFim] (formato yyyy-MM-dd).
    Formatos suportados: csv (UTF-8 com BOM, abre certo no Excel) e xml.
*/
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatorioController : ControllerBase
{
    private readonly IReportService _reportService;

    public RelatorioController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Catálogo de relatórios visíveis pro usuário logado, agrupado por tipo.</summary>
    [HttpGet("catalogo")]
    public IActionResult Catalogo()
    {
        var (podeOficina, podeConcessionaria) = ObterPermissoes();
        return Ok(_reportService.ObterCatalogo(podeOficina, podeConcessionaria));
    }

    // /api/relatorio/export/{id}?formato=csv&dataInicio=2026-01-01&dataFim=2026-06-30
    [HttpGet("export/{id}")]
    public async Task<IActionResult> Export(
        string id,
        [FromQuery] string formato,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        var (podeOficina, podeConcessionaria) = ObterPermissoes();

        var r = await _reportService.ExportAsync(id, formato, dataInicio, dataFim, podeOficina, podeConcessionaria);
        if (!r.IsSuccess) return BadRequest(r.Error);

        var contentType = formato.ToLowerInvariant() == "xml" ? "application/xml; charset=utf-8" : "text/csv; charset=utf-8";
        var fileName = $"relatorio-{id}-{DateTime.Now:yyyyMMdd-HHmm}.{formato.ToLowerInvariant()}";
        return File(r.Value!, contentType, fileName);
    }

    private (bool PodeOficina, bool PodeConcessionaria) ObterPermissoes()
    {
        var podeOficina = User.IsInRole("Admin") || User.IsInRole("ChefeOficina");
        var podeConcessionaria = User.IsInRole("Admin") || User.IsInRole("GerenteVendas");
        return (podeOficina, podeConcessionaria);
    }
}
