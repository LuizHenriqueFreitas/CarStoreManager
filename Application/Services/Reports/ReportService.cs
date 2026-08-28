using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;
using CarStoreManager.Application.DTOs.Reports;
using CarStoreManager.Application.Interfaces;

namespace CarStoreManager.Application.Services.Reports;

/// <summary>
/// Monta relatórios consolidados a partir das mesmas métricas usadas nos
/// gráficos da dashboard (IDashboardService) e delega a formatação (CSV/XML)
/// via Strategy.
/// </summary>
public class ReportService : IReportService
{
    private readonly IDashboardService _dashboard;
    private readonly CsvReportFormatter _csvFormatter;
    private readonly XmlReportFormatter _xmlFormatter;

    public ReportService(
        IDashboardService dashboard,
        CsvReportFormatter csvFormatter,
        XmlReportFormatter xmlFormatter)
    {
        _dashboard = dashboard;
        _csvFormatter = csvFormatter;
        _xmlFormatter = xmlFormatter;
    }

    public async Task<Result<byte[]>> ExportAsync(ReportType type, string format, DateTime dataInicio, DateTime dataFim)
    {
        var rMetricas = await _dashboard.ObterMetricasPeriodoAsync(dataInicio, dataFim);
        if (!rMetricas.IsSuccess || rMetricas.Value is null)
            return Result<byte[]>.Fail(rMetricas.Error ?? "Não foi possível obter as métricas.");

        var data = BuildReportData(type, rMetricas.Value);
        data.Periodo = $"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}";

        IReportFormatter formatter = format.ToLowerInvariant() switch
        {
            "csv" => _csvFormatter,
            "xml" => _xmlFormatter,
            _ => throw new InvalidOperationException($"Formato '{format}' não suportado.")
        };

        var bytes = await formatter.FormatAsync(data);
        return Result<byte[]>.Ok(bytes);
    }

    private static ReportData BuildReportData(ReportType type, DashboardMetricasDTO m)
    {
        var data = new ReportData
        {
            Title = type switch
            {
                ReportType.OficinaCompleto => "Oficina — Relatório Completo",
                ReportType.ConcessionariaCompleto => "Concessionária — Relatório Completo",
                ReportType.Geral => "Relatório Geral",
                _ => "Relatório"
            },
            GeneratedAt = DateTime.Now
        };

        switch (type)
        {
            case ReportType.OficinaCompleto:
                data.Sections.Add(new ReportSection
                {
                    Name = "Financeiro da oficina no período",
                    Rows = new()
                    {
                        LinhaMetrica("Receita de serviços", m.ReceitaServicosMesAtual),
                        LinhaMetrica("Despesas da oficina (estimativa proporcional ao período)", m.TotalDespesasOficinaMensal),
                        LinhaMetrica("Lucro operacional", m.LucroOficinaMes)
                    }
                });
                data.Sections.Add(SecaoContagem("Ordens de serviço por status", m.OrdensServicoPorStatus));
                data.Sections.Add(new ReportSection
                {
                    Name = "Receita por mecânico (top 5)",
                    Rows = m.ReceitaPorMecanico
                        .Select(r => new Dictionary<string, object?>
                        {
                            ["Mecanico"] = r.MecanicoNome,
                            ["Receita"] = r.Receita
                        }).ToList()
                });
                data.Sections.Add(SecaoSerieMensal("Receita de serviços por mês, no período", m.SerieReceitaServicos));
                data.Sections.Add(SecaoSerieMensal("Receita de serviços acumulada no período", m.SerieReceitaServicosAcumulada12m));
                break;

            case ReportType.ConcessionariaCompleto:
                data.Sections.Add(new ReportSection
                {
                    Name = "Financeiro da concessionária no período",
                    Rows = new()
                    {
                        LinhaMetrica("Receita de vendas", m.ReceitaVendasMesAtual),
                        LinhaMetrica("Despesas da concessionária (estimativa proporcional ao período)", m.TotalDespesasConcessionariaMensal),
                        LinhaMetrica("Lucro operacional", m.LucroConcessionariaMes),
                        LinhaMetrica("Capital imobilizado em veículos (estoque atual)", m.CapitalEstoqueVeiculos)
                    }
                });
                data.Sections.Add(SecaoContagem("Veículos por status (estoque atual)", m.VeiculosPorStatus));
                data.Sections.Add(new ReportSection
                {
                    Name = "Vendas por marca no período (top 5)",
                    Rows = m.VendasPorMarca
                        .Select(v => new Dictionary<string, object?>
                        {
                            ["Marca"] = v.Marca,
                            ["Quantidade"] = v.Quantidade
                        }).ToList()
                });
                data.Sections.Add(SecaoSerieMensal("Receita de vendas por mês, no período", m.SerieReceitaVendas));
                data.Sections.Add(new ReportSection
                {
                    Name = "Propostas aprovadas vs rejeitadas, no período",
                    Rows = m.PropostasTimeline
                        .Select(t => new Dictionary<string, object?>
                        {
                            ["Mes"] = t.MesLabel,
                            ["Aprovadas"] = t.Aprovadas,
                            ["Rejeitadas"] = t.Rejeitadas
                        }).ToList()
                });
                break;

            case ReportType.Geral:
                data.Sections.Add(new ReportSection
                {
                    Name = "KPIs financeiros do período",
                    Rows = new()
                    {
                        LinhaMetrica("Receitas do período", m.TotalReceitasMes),
                        LinhaMetrica("Despesas do período (estimativa proporcional)", m.TotalDespesasMes),
                        LinhaMetrica("Lucro líquido", m.LucroLiquidoMes),
                        LinhaMetrica("Capital imobilizado em veículos (estoque atual)", m.CapitalEstoqueVeiculos)
                    }
                });
                data.Sections.Add(new ReportSection
                {
                    Name = "Receita Oficina vs Concessionária, por mês no período",
                    Rows = m.SerieReceitaServicos
                        .Zip(m.SerieReceitaVendas, (of, co) => new Dictionary<string, object?>
                        {
                            ["Mes"] = of.MesLabel,
                            ["Oficina"] = of.Valor,
                            ["Concessionaria"] = co.Valor
                        }).ToList()
                });
                data.Sections.Add(SecaoContagem("Ordens de serviço por status, no período", m.OrdensServicoPorStatus));
                data.Sections.Add(SecaoContagem("Veículos por status (estoque atual)", m.VeiculosPorStatus));
                break;
        }

        return data;
    }

    private static Dictionary<string, object?> LinhaMetrica(string nome, decimal valor)
        => new() { ["Metrica"] = nome, ["Valor"] = valor };

    private static ReportSection SecaoContagem(string nome, Dictionary<string, int> contagem)
        => new()
        {
            Name = nome,
            Rows = contagem
                .Select(kvp => new Dictionary<string, object?> { ["Status"] = kvp.Key, ["Quantidade"] = kvp.Value })
                .ToList()
        };

    private static ReportSection SecaoSerieMensal(string nome, List<MesValorDTO> serie)
        => new()
        {
            Name = nome,
            Rows = serie
                .Select(s => new Dictionary<string, object?> { ["Mes"] = s.MesLabel, ["Valor"] = s.Valor })
                .ToList()
        };
}
