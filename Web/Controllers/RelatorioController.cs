using CarStoreManager.Application.DTOs.Reports;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarStoreManager.Web.Controllers;

/*
    Endpoints que geram relatórios em CSV/XML (UTF-8 com BOM no CSV, para abrir
    bem no Excel). Acessíveis por Admin via UI da Dashboard ou direto pela URL
    com cookie/Bearer. Todo relatório aceita um intervalo [dataInicio, dataFim]
    opcional (formato yyyy-MM-dd) — se omitido, traz todos os registros.
*/
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatorioController : ControllerBase
{
    private readonly IVeiculoVendaService _veiculoService;
    private readonly IOrdemServicoService _ordemService;
    private readonly IPropostaVendaService _propostaService;
    private readonly IClienteService _clienteService;
    private readonly IMecanicoService _mecanicoService;
    private readonly IReportService _reportService;
    private readonly CsvReportFormatter _csvFormatter;
    private readonly XmlReportFormatter _xmlFormatter;

    public RelatorioController(
        IVeiculoVendaService veiculoService,
        IOrdemServicoService ordemService,
        IPropostaVendaService propostaService,
        IClienteService clienteService,
        IMecanicoService mecanicoService,
        IReportService reportService,
        CsvReportFormatter csvFormatter,
        XmlReportFormatter xmlFormatter)
    {
        _veiculoService = veiculoService;
        _ordemService = ordemService;
        _propostaService = propostaService;
        _clienteService = clienteService;
        _mecanicoService = mecanicoService;
        _reportService = reportService;
        _csvFormatter = csvFormatter;
        _xmlFormatter = xmlFormatter;
    }

    // ===== Relatórios consolidados (mesmas métricas dos gráficos da dashboard,
    // recalculadas para o período escolhido) =====
    // /api/relatorio/export?tipo=OficinaCompleto&formato=csv&dataInicio=2026-01-01&dataFim=2026-06-30
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string tipo,
        [FromQuery] string formato,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        if (!Enum.TryParse<ReportType>(tipo, ignoreCase: true, out var tipoRelatorio))
            return BadRequest($"Tipo de relatório inválido: {tipo}");

        if (formato is not ("csv" or "xml"))
            return BadRequest($"Formato não suportado: {formato}. Use 'csv' ou 'xml'.");

        if (dataFim.Date < dataInicio.Date)
            return BadRequest("Data final não pode ser anterior à data inicial.");

        var podeVerOficina = User.IsInRole("Admin") || User.IsInRole("ChefeOficina");
        var podeVerConcessionaria = User.IsInRole("Admin") || User.IsInRole("GerenteVendas");
        var permitido = tipoRelatorio switch
        {
            ReportType.OficinaCompleto => podeVerOficina,
            ReportType.ConcessionariaCompleto => podeVerConcessionaria,
            ReportType.Geral => podeVerOficina && podeVerConcessionaria,
            _ => false
        };
        if (!permitido) return Forbid();

        var r = await _reportService.ExportAsync(tipoRelatorio, formato, dataInicio, dataFim);
        if (!r.IsSuccess) return BadRequest(r.Error);

        var contentType = formato == "xml" ? "application/xml; charset=utf-8" : "text/csv; charset=utf-8";
        var fileName = $"relatorio-{tipo.ToLowerInvariant()}-{DateTime.Now:yyyyMMdd-HHmm}.{formato}";
        return File(r.Value!, contentType, fileName);
    }

    // ===== ÁREA: CONCESSIONÁRIA =====
    [HttpGet("veiculos-venda")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> VeiculosVenda(
        [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] string formato = "csv")
    {
        var r = await _veiculoService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var itens = FiltrarPorPeriodo(r.Value!, v => v.DataCriacao, dataInicio, dataFim);

        var data = NovoReportData("Veículos à venda", dataInicio, dataFim, "Veículos à venda", itens, v =>
            new Dictionary<string, object?>
            {
                ["Marca"] = v.Marca,
                ["Modelo"] = v.Modelo,
                ["Ano"] = v.Ano,
                ["Combustivel"] = v.Combustivel,
                ["Disponibilidade"] = v.Disponibilidade,
                ["Valor"] = v.Valor,
                ["DataCriacao"] = v.DataCriacao
            });

        return await Formatar(data, formato, "veiculos-venda");
    }

    // ===== ÁREA: OFICINA =====
    [HttpGet("ordens-servico")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> OrdensServico(
        [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] string formato = "csv")
    {
        var r = await _ordemService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var itens = FiltrarPorPeriodo(r.Value!, o => o.DataCriacao, dataInicio, dataFim);

        var data = NovoReportData("Ordens de serviço", dataInicio, dataFim, "Ordens de serviço", itens, o =>
            new Dictionary<string, object?>
            {
                ["NumeroPublico"] = o.NumeroPublico,
                ["Tipo"] = o.Tipo,
                ["Status"] = o.Status,
                ["PrazoEstimado"] = o.PrazoEstimado,
                ["ValorTotal"] = o.ValorTotal,
                ["DataCriacao"] = o.DataCriacao
            });

        return await Formatar(data, formato, "ordens-servico");
    }

    // ===== ÁREA: CONCESSIONÁRIA =====
    [HttpGet("propostas-venda")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> PropostasVenda(
        [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] string formato = "csv")
    {
        var r = await _propostaService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var itens = FiltrarPorPeriodo(r.Value!, p => p.DataCriacao, dataInicio, dataFim);

        var data = NovoReportData("Propostas de venda", dataInicio, dataFim, "Propostas de venda", itens, p =>
            new Dictionary<string, object?>
            {
                ["VeiculoId"] = p.VeiculoVendaId,
                ["ClienteId"] = p.ClienteId,
                ["ValorFinal"] = p.ValorFinal,
                ["Status"] = p.Status,
                ["DataCriacao"] = p.DataCriacao
            });

        return await Formatar(data, formato, "propostas-venda");
    }

    // Clientes são compartilhados entre as duas áreas (cliente compra carro e
    // também faz manutenção). Liberado para os dois gestores.
    [HttpGet("clientes")]
    [Authorize(Roles = "Admin,ChefeOficina,GerenteVendas")]
    public async Task<IActionResult> Clientes(
        [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] string formato = "csv")
    {
        var r = await _clienteService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var itens = FiltrarPorPeriodo(r.Value!, c => c.DataCriacao, dataInicio, dataFim);

        var data = NovoReportData("Clientes", dataInicio, dataFim, "Clientes", itens, c =>
            new Dictionary<string, object?>
            {
                ["Nome"] = c.Nome,
                ["CPF"] = c.Cpf,
                ["Telefone"] = c.Telefone,
                ["Email"] = c.Email,
                ["DataCriacao"] = c.DataCriacao
            });

        return await Formatar(data, formato, "clientes");
    }

    // ===== ÁREA: OFICINA =====
    [HttpGet("mecanicos")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> Mecanicos(
        [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] string formato = "csv")
    {
        var r = await _mecanicoService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var itens = FiltrarPorPeriodo(r.Value!, m => m.DataCriacao, dataInicio, dataFim);

        var data = NovoReportData("Mecânicos", dataInicio, dataFim, "Mecânicos", itens, m =>
            new Dictionary<string, object?>
            {
                ["Nome"] = m.Nome,
                ["Especialidade"] = m.Especialidade,
                ["Nivel"] = m.Nivel,
                ["DataCriacao"] = m.DataCriacao
            });

        return await Formatar(data, formato, "mecanicos");
    }

    // =========================
    // HELPERS PRIVADOS
    // =========================

    private static List<T> FiltrarPorPeriodo<T>(
        IEnumerable<T> itens, Func<T, DateTime> dataSeletor, DateTime? inicio, DateTime? fim)
    {
        var q = itens.AsEnumerable();
        if (inicio.HasValue)
            q = q.Where(i => dataSeletor(i) >= inicio.Value.Date);
        if (fim.HasValue)
            q = q.Where(i => dataSeletor(i) <= fim.Value.Date.AddDays(1).AddTicks(-1));
        return q.ToList();
    }

    private static ReportData NovoReportData<T>(
        string titulo, DateTime? dataInicio, DateTime? dataFim, string nomeSecao,
        List<T> itens, Func<T, Dictionary<string, object?>> linha)
    {
        return new ReportData
        {
            Title = titulo,
            GeneratedAt = DateTime.Now,
            Periodo = dataInicio.HasValue && dataFim.HasValue
                ? $"{dataInicio.Value:dd/MM/yyyy} a {dataFim.Value:dd/MM/yyyy}"
                : "",
            Sections = new List<ReportSection>
            {
                new() { Name = nomeSecao, Rows = itens.Select(linha).ToList() }
            }
        };
    }

    private async Task<IActionResult> Formatar(ReportData data, string formato, string nomeArquivoBase)
    {
        if (formato is not ("csv" or "xml"))
            return BadRequest($"Formato não suportado: {formato}. Use 'csv' ou 'xml'.");

        var bytes = formato == "xml"
            ? await _xmlFormatter.FormatAsync(data)
            : await _csvFormatter.FormatAsync(data);

        var contentType = formato == "xml" ? "application/xml; charset=utf-8" : "text/csv; charset=utf-8";
        var fileName = $"{nomeArquivoBase}-{DateTime.Now:yyyyMMdd-HHmm}.{formato}";
        return File(bytes, contentType, fileName);
    }
}
