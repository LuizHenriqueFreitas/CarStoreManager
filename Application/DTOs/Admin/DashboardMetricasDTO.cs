namespace CarStoreManager.Application.DTOs.Admin;

public class DashboardMetricasDTO
{
    // === KPIs financeiros do mês corrente ===
    /// <summary>Soma de TODAS as despesas mensais ativas (todos os setores).</summary>
    public decimal TotalDespesasFixasMensal { get; set; }

    /// <summary>Despesas mensais ativas com setor = Geral (compartilhadas).</summary>
    public decimal TotalDespesasGeralMensal { get; set; }

    /// <summary>Despesas mensais ativas com setor = Oficina (exclusivas do setor).</summary>
    public decimal TotalDespesasOficinaMensal { get; set; }

    /// <summary>Despesas mensais ativas com setor = Concessionaria (exclusivas do setor).</summary>
    public decimal TotalDespesasConcessionariaMensal { get; set; }

    public decimal GastoPecasMesAtual { get; set; }
    public decimal CapitalEstoqueVeiculos { get; set; }
    public decimal ReceitaServicosMesAtual { get; set; }
    public decimal ReceitaVendasMesAtual { get; set; }

    public decimal TotalReceitasMes => ReceitaServicosMesAtual + ReceitaVendasMesAtual;
    public decimal TotalDespesasMes => TotalDespesasFixasMensal + GastoPecasMesAtual;
    public decimal LucroLiquidoMes => TotalReceitasMes - TotalDespesasMes;

    /// <summary>
    /// Lucro operacional da oficina (mês corrente) — receita de serviços menos
    /// gasto direto com peças menos despesas exclusivas da oficina. Despesas
    /// Gerais (compartilhadas) ficam só no resumo Geral.
    /// </summary>
    public decimal LucroOficinaMes => ReceitaServicosMesAtual - GastoPecasMesAtual - TotalDespesasOficinaMensal;

    /// <summary>
    /// Lucro operacional da concessionária (mês corrente) — receita de vendas
    /// menos despesas exclusivas da concessionária. CapitalEstoqueVeiculos NÃO
    /// entra: é dinheiro parado em estoque, não saída de caixa do mês.
    /// </summary>
    public decimal LucroConcessionariaMes => ReceitaVendasMesAtual - TotalDespesasConcessionariaMensal;

    // === Distribuições para gráficos ===
    public Dictionary<string, int> OrdensServicoPorStatus { get; set; } = new();

    public List<MesValorDTO> SerieReceitaServicos { get; set; } = new();
    public List<MesValorDTO> SerieReceitaVendas { get; set; } = new();
    public List<MesValorDTO> SerieGastoPecas { get; set; } = new();
}

public class MesValorDTO
{
    public string MesLabel { get; set; } = "";
    public decimal Valor { get; set; }
}
