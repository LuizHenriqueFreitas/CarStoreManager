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
[Authorize]
public class RelatorioController : ControllerBase
{
    private readonly IVeiculoVendaService _veiculoService;
    private readonly IOrdemServicoService _ordemService;
    private readonly IPropostaVendaService _propostaService;
    private readonly IClienteService _clienteService;
    private readonly IMecanicoService _mecanicoService;
    private readonly IDashboardService _dashboardService;

    public RelatorioController(
        IVeiculoVendaService veiculoService,
        IOrdemServicoService ordemService,
        IPropostaVendaService propostaService,
        IClienteService clienteService,
        IMecanicoService mecanicoService,
        IDashboardService dashboardService)
    {
        _veiculoService = veiculoService;
        _ordemService = ordemService;
        _propostaService = propostaService;
        _clienteService = clienteService;
        _mecanicoService = mecanicoService;
        _dashboardService = dashboardService;
    }

    // ===== FLUXO DE CAIXA CONSOLIDADO (período escolhido pelo admin) =====
    /// <summary>
    /// Relatório CSV multi-seção que consolida receitas, despesas, lucro por setor
    /// e evolução mês a mês — para o intervalo <c>?inicio=yyyy-MM-dd&amp;fim=yyyy-MM-dd</c>.
    /// Quando os parâmetros são omitidos, assume o mês corrente como padrão razoável.
    /// </summary>
    [HttpGet("fluxo-caixa")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FluxoCaixa([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
    {
        // Defaults: primeiro dia do mês corrente até hoje. Cliente da Dashboard
        // sempre manda explícito — esse fallback existe pra quem chama a URL direta.
        var hoje = DateTime.Today;
        var dataInicio = inicio?.Date ?? new DateTime(hoje.Year, hoje.Month, 1);
        var dataFim = fim?.Date ?? hoje;

        var r = await _dashboardService.ObterFluxoCaixaPorPeriodoAsync(dataInicio, dataFim);
        if (!r.IsSuccess) return BadRequest(r.Error);
        var m = r.Value!;
        var ptBr = CultureInfo.GetCultureInfo("pt-BR");
        string F(decimal v) => v.ToString("N2", ptBr);

        var sb = new StringBuilder();

        sb.AppendLine("RELATÓRIO DE FLUXO DE CAIXA — CarStore Manager");
        sb.AppendLine($"Gerado em;{DateTime.Now.ToString("dd/MM/yyyy HH:mm", ptBr)}");
        sb.AppendLine($"Período;{m.PeriodoInicio:dd/MM/yyyy} a {m.PeriodoFim:dd/MM/yyyy}");
        sb.AppendLine($"Meses-calendário tocados;{m.QuantidadeMeses}");
        sb.AppendLine();

        // === RESUMO DO PERÍODO ===
        sb.AppendLine("=== RESUMO DO PERÍODO ===");
        sb.AppendLine("Indicador;Valor (R$)");
        sb.AppendLine($"Receita de serviços (oficina);{F(m.ReceitaServicos)}");
        sb.AppendLine($"Receita de vendas (concessionária);{F(m.ReceitaVendas)}");
        sb.AppendLine($"TOTAL DE RECEITAS;{F(m.TotalReceitas)}");
        sb.AppendLine();
        foreach (var (tipo, valorMensal) in m.DespesasFixasPorTipoMensal)
            sb.AppendLine($"Despesas fixas — {Csv(tipo)} ({m.QuantidadeMeses}x mensal);{F(valorMensal * m.QuantidadeMeses)}");
        sb.AppendLine($"Despesas fixas — TOTAL ({m.QuantidadeMeses}x mensal);{F(m.DespesasFixasTotalPeriodo)}");
        sb.AppendLine($"Gasto com peças (notas fiscais aprovadas);{F(m.GastoPecas)}");
        sb.AppendLine($"Compra de material (aquisição de veículos);{F(m.CompraMaterial)}");
        sb.AppendLine($"TOTAL DE DESPESAS;{F(m.TotalDespesas)}");
        sb.AppendLine();
        sb.AppendLine($"LUCRO LÍQUIDO DO PERÍODO;{F(m.LucroLiquido)}");
        sb.AppendLine();

        // === LUCRO POR SETOR ===
        // As despesas fixas não são mais rateadas por setor (agora classificadas por
        // tipo); por isso o lucro por setor considera só os custos diretos. As
        // despesas fixas entram no Lucro Líquido total, não no lucro setorial.
        var receitaOficina = m.ReceitaServicos;
        var despesaOficina = m.GastoPecas;
        var receitaConce = m.ReceitaVendas;
        var despesaConce = m.CompraMaterial;

        sb.AppendLine("=== LUCRO POR SETOR (custos diretos) ===");
        sb.AppendLine("Setor;Receitas (R$);Despesas (R$);Lucro (R$)");
        sb.AppendLine($"Oficina;{F(receitaOficina)};{F(despesaOficina)};{F(m.LucroOficina)}");
        sb.AppendLine($"Concessionária;{F(receitaConce)};{F(despesaConce)};{F(m.LucroConcessionaria)}");
        sb.AppendLine($"Despesas fixas (não alocadas a setor);0,00;{F(m.DespesasFixasTotalPeriodo)};{F(-m.DespesasFixasTotalPeriodo)}");
        sb.AppendLine();

        // === EVOLUÇÃO MÊS A MÊS ===
        sb.AppendLine("=== EVOLUÇÃO MÊS A MÊS ===");
        sb.AppendLine("Mês;Receita serviços (R$);Receita vendas (R$);Gasto peças (R$);Compra material (R$);Despesas fixas (R$);Saldo do mês (R$)");
        for (int i = 0; i < m.SerieReceitaServicos.Count; i++)
        {
            var rs = m.SerieReceitaServicos[i].Valor;
            var rv = i < m.SerieReceitaVendas.Count ? m.SerieReceitaVendas[i].Valor : 0m;
            var gp = i < m.SerieGastoPecas.Count ? m.SerieGastoPecas[i].Valor : 0m;
            var cm = i < m.SerieCompraMaterial.Count ? m.SerieCompraMaterial[i].Valor : 0m;
            var df = m.DespesasFixasTotalMensal; // valor recorrente
            var saldo = rs + rv - gp - cm - df;
            sb.AppendLine($"{Csv(m.SerieReceitaServicos[i].MesLabel)};{F(rs)};{F(rv)};{F(gp)};{F(cm)};{F(df)};{F(saldo)}");
        }
        sb.AppendLine();

        // === CAPITAL IMOBILIZADO (não-caixa) ===
        sb.AppendLine("=== CAPITAL IMOBILIZADO (referência — NÃO compõe fluxo de caixa) ===");
        sb.AppendLine("Indicador;Valor (R$)");
        sb.AppendLine($"Veículos disponíveis em estoque (snapshot atual);{F(m.CapitalEstoqueVeiculos)}");
        sb.AppendLine("Observação;Dinheiro investido em estoque — só vira receita quando o veículo for vendido.");

        var nome = $"fluxo-caixa_{m.PeriodoInicio:yyyyMMdd}_{m.PeriodoFim:yyyyMMdd}";
        return Csv(sb, nome);
    }

    // ===== ÁREA: CONCESSIONÁRIA =====
    [HttpGet("veiculos-venda")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> VeiculosVenda()
    {
        var r = await _veiculoService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("Marca;Modelo;Ano;Combustivel;Disponibilidade;CustoAquisicao;Valor");
        foreach (var v in r.Value!)
            sb.AppendLine($"{Csv(v.Marca)};{Csv(v.Modelo)};{v.Ano};{Csv(v.Combustivel)};{Csv(v.Disponibilidade)};{v.CustoAquisicao.ToString("F2", CultureInfo.InvariantCulture)};{v.Valor.ToString("F2", CultureInfo.InvariantCulture)}");

        return Csv(sb, "veiculos-venda");
    }

    // ===== ÁREA: OFICINA =====
    [HttpGet("ordens-servico")]
    [Authorize(Roles = "Admin,ChefeOficina")]
    public async Task<IActionResult> OrdensServico()
    {
        var r = await _ordemService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("NumeroPublico;Tipo;Status;PrazoEstimado;ValorTotal");
        foreach (var o in r.Value!)
            sb.AppendLine($"{Csv(o.NumeroPublico)};{Csv(o.Tipo)};{Csv(o.Status)};{o.PrazoEstimado:yyyy-MM-dd};{o.ValorTotal.ToString("F2", CultureInfo.InvariantCulture)}");

        return Csv(sb, "ordens-servico");
    }

    // ===== ÁREA: CONCESSIONÁRIA =====
    [HttpGet("propostas-venda")]
    [Authorize(Roles = "Admin,GerenteVendas")]
    public async Task<IActionResult> PropostasVenda()
    {
        var r = await _propostaService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("VeiculoId;ClienteId;ValorFinal;Status;DataCriacao");
        foreach (var p in r.Value!)
            sb.AppendLine($"{p.VeiculoVendaId};{p.ClienteId};{p.ValorFinal.ToString("F2", CultureInfo.InvariantCulture)};{Csv(p.Status)};{p.DataCriacao:yyyy-MM-dd HH:mm}");

        return Csv(sb, "propostas-venda");
    }

    // Clientes são compartilhados entre as duas áreas (cliente compra carro e
    // também faz manutenção). Liberado para os dois gestores.
    [HttpGet("clientes")]
    [Authorize(Roles = "Admin,ChefeOficina,GerenteVendas")]
    public async Task<IActionResult> Clientes()
    {
        var r = await _clienteService.GetAllAsync();
        if (!r.IsSuccess) return BadRequest(r.Error);

        var sb = NovoCsv("Nome;CPF;Telefone;Email");
        foreach (var c in r.Value!)
            sb.AppendLine($"{Csv(c.Nome)};{Csv(c.Cpf)};{Csv(c.Telefone)};{Csv(c.Email)}");

        return Csv(sb, "clientes");
    }

    // ===== ÁREA: OFICINA =====
    [HttpGet("mecanicos")]
    [Authorize(Roles = "Admin,ChefeOficina")]
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
