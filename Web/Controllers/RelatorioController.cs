using System.Globalization;
using System.Text;
using CarStoreManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

/*
    Endpoints que geram relatórios em CSV (UTF-8 com BOM para abrir bem no Excel).
    Acessíveis por Admin via UI da Dashboard ou direto pela URL com cookie/Bearer.
*/
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RelatorioController : ControllerBase
{
    private readonly IVeiculoVendaService _veiculoService;
    private readonly IOrdemServicoService _ordemService;
    private readonly IPropostaVendaService _propostaService;
    private readonly IClienteService _clienteService;
    private readonly IMecanicoService _mecanicoService;

    public RelatorioController(
        IVeiculoVendaService veiculoService,
        IOrdemServicoService ordemService,
        IPropostaVendaService propostaService,
        IClienteService clienteService,
        IMecanicoService mecanicoService)
    {
        _veiculoService = veiculoService;
        _ordemService = ordemService;
        _propostaService = propostaService;
        _clienteService = clienteService;
        _mecanicoService = mecanicoService;
    }

    [HttpGet("veiculos-venda")]
    public async Task<IActionResult> VeiculosVenda()
    {
        var r = await _veiculoService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("Marca;Modelo;Ano;Combustivel;Disponibilidade;Valor");
        foreach (var v in r.Value!)
            sb.AppendLine($"{Csv(v.Marca)};{Csv(v.Modelo)};{v.Ano};{Csv(v.Combustivel)};{Csv(v.Disponibilidade)};{v.Valor.ToString("F2", CultureInfo.InvariantCulture)}");

        return Csv(sb, "veiculos-venda");
    }

    [HttpGet("ordens-servico")]
    public async Task<IActionResult> OrdensServico()
    {
        var r = await _ordemService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("NumeroPublico;Tipo;Status;PrazoEstimado;ValorTotal");
        foreach (var o in r.Value!)
            sb.AppendLine($"{Csv(o.NumeroPublico)};{Csv(o.Tipo)};{Csv(o.Status)};{o.PrazoEstimado:yyyy-MM-dd};{o.ValorTotal.ToString("F2", CultureInfo.InvariantCulture)}");

        return Csv(sb, "ordens-servico");
    }

    [HttpGet("propostas-venda")]
    public async Task<IActionResult> PropostasVenda()
    {
        var r = await _propostaService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("VeiculoId;ClienteId;ValorFinal;Status;DataCriacao");
        foreach (var p in r.Value!)
            sb.AppendLine($"{p.VeiculoVendaId};{p.ClienteId};{p.ValorFinal.ToString("F2", CultureInfo.InvariantCulture)};{Csv(p.Status)};{p.DataCriacao:yyyy-MM-dd HH:mm}");

        return Csv(sb, "propostas-venda");
    }

    [HttpGet("clientes")]
    public async Task<IActionResult> Clientes()
    {
        var r = await _clienteService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("Nome;CPF;Telefone;Email");
        foreach (var c in r.Value!)
            sb.AppendLine($"{Csv(c.Nome)};{Csv(c.Cpf)};{Csv(c.Telefone)};{Csv(c.Email)}");

        return Csv(sb, "clientes");
    }

    [HttpGet("mecanicos")]
    public async Task<IActionResult> Mecanicos()
    {
        var r = await _mecanicoService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("Nome;Especialidade;Nivel");
        foreach (var m in r.Value!)
            sb.AppendLine($"{Csv(m.Nome)};{Csv(m.Especialidade)};{Csv(m.Nivel)}");

        return Csv(sb, "mecanicos");
    }

    private static StringBuilder NovoCsv(string header)
    {
        var sb = new StringBuilder();
        sb.AppendLine(header);
        return sb;
    }

    private static string Csv(string? v) => v is null ? string.Empty : v.Replace(";", ",").Replace("\n", " ");

    private FileResult Csv(StringBuilder sb, string nome)
    {
        // BOM UTF-8 garante acentuação correta no Excel.
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var bytes = bom.Concat(body).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"{nome}-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }
}
