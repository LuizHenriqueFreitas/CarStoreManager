using CarStoreManager.Application.Interfaces.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers.Sistema;

/// <summary>
/// Central de Documentos — só leitura, um endpoint por tipo de documento.
/// Termos/contratos da concessionária ficam pra Admin/GerenteVendas;
/// contrato de OS (oficina) fica pra Admin/ChefeOficina.
/// </summary>
[ApiController]
[Route("api/documentos")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentosService _service;

    public DocumentosController(IDocumentosService service) => _service = service;

    [HttpGet("termos-entrega")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> TermosEntrega()
    {
        var r = await _service.ListarTermosEntregaAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("termos-test-drive")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> TermosTestDrive()
    {
        var r = await _service.ListarTermosTestDriveAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("contratos-consignacao")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> ContratosConsignacao()
    {
        var r = await _service.ListarContratosConsignacaoAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }

    [HttpGet("contratos-os")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> ContratosOS()
    {
        var r = await _service.ListarContratosOSAsync();
        return r.IsSuccess ? Ok(r.Value) : BadRequest(r.Error);
    }
}
