namespace CarStoreManager.Application.DTOs.Admin;

public class DashboardMetricasDTO
{
    // === KPIs financeiros do mês corrente ===
    /// <summary>Soma de TODAS as despesas mensais ativas.</summary>
    public decimal TotalDespesasFixasMensal { get; set; }

    /// <summary>Despesas mensais ativas agrupadas por tipo (nome do tipo → total).</summary>
    public Dictionary<string, decimal> DespesasPorTipo { get; set; } = new();

    public decimal GastoPecasMesAtual { get; set; }

    /// <summary>
    /// Gasto da concessionária com a compra de veículos cadastrados no mês
    /// corrente ("Compra de material") — análogo ao GastoPecas da oficina.
    /// </summary>
    public decimal CompraMaterialMesAtual { get; set; }

    public decimal CapitalEstoqueVeiculos { get; set; }
    public decimal ReceitaServicosMesAtual { get; set; }
    public decimal ReceitaVendasMesAtual { get; set; }

    public decimal TotalReceitasMes => ReceitaServicosMesAtual + ReceitaVendasMesAtual;
    public decimal TotalDespesasMes => TotalDespesasFixasMensal + GastoPecasMesAtual + CompraMaterialMesAtual;
    public decimal LucroLiquidoMes => TotalReceitasMes - TotalDespesasMes;

    /// <summary>
    /// Lucro operacional da oficina (mês corrente) — receita de serviços menos o
    /// gasto direto com peças. As despesas fixas não são mais rateadas por setor;
    /// entram só no resumo geral.
    /// </summary>
    public decimal LucroOficinaMes => ReceitaServicosMesAtual - GastoPecasMesAtual;

    /// <summary>
    /// Lucro operacional da concessionária (mês corrente) — receita de vendas
    /// menos a compra de material (aquisição de veículos). CapitalEstoqueVeiculos
    /// NÃO entra: é dinheiro parado em estoque, não saída de caixa do mês.
    /// </summary>
    public decimal LucroConcessionariaMes =>
        ReceitaVendasMesAtual - CompraMaterialMesAtual;

    // === Distribuições para gráficos ===
    public Dictionary<string, int> OrdensServicoPorStatus { get; set; } = new();

    public List<MesValorDTO> SerieReceitaServicos { get; set; } = new();
    public List<MesValorDTO> SerieReceitaVendas { get; set; } = new();
    public List<MesValorDTO> SerieGastoPecas { get; set; } = new();
    public List<MesValorDTO> SerieCompraMaterial { get; set; } = new();
}

public class MesValorDTO
{
    public string MesLabel { get; set; } = "";
    public decimal Valor { get; set; }
}
