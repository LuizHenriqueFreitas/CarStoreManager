using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Recepcionista")]
public class NotaFiscalVendaOSController : ControllerBase
{
    private readonly INotaFiscalVendaOSRepository _repo;
    public NotaFiscalVendaOSController(INotaFiscalVendaOSRepository repo) => _repo = repo;

    /// <summary>NF de venda gerada ao finalizar uma OS específica.</summary>
    [HttpGet("/api/ordemservico/{ordemId:guid}/nota-venda")]
    public async Task<IActionResult> ObterPorOrdem(Guid ordemId)
    {
        var nf = await _repo.ObterPorOrdemAsync(ordemId);
        if (nf is null) return NotFound("Nota não emitida para esta OS.");
        return Ok(new
        {
            nf.Id,
            nf.OrdemServicoId,
            nf.ClienteId,
            nf.Numero,
            nf.DataEmissao,
            nf.DataCancelamento,
            nf.MotivoCancelamento,
            ValorServico = nf.ValorServico.GetValorDinheiro(),
            ValorPecas = nf.ValorPecas.GetValorDinheiro(),
            ValorTotal = nf.ValorTotal.GetValorDinheiro(),
            Cancelada = nf.Cancelada,
            // JSON cru — frontend desserializa
            ItensJson = nf.ItensJson,
            ClienteSnapshotJson = nf.ClienteSnapshotJson
        });
    }

    [HttpGet("/api/notas-venda")]
    public async Task<IActionResult> Listar()
    {
        var lista = await _repo.GetAllAsync();
        return Ok(lista.Select(nf => new
        {
            nf.Id,
            nf.Numero,
            nf.OrdemServicoId,
            nf.ClienteId,
            nf.DataEmissao,
            ValorTotal = nf.ValorTotal.GetValorDinheiro(),
            nf.Cancelada
        }));
    }
}
