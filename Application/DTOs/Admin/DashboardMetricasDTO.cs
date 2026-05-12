namespace CarStoreManager.Application.DTOs.Admin;

public class DashboardMetricasDTO
{
    // === KPIs financeiros do mês corrente ===
    public decimal TotalSalariosMensal { get; set; }
    public decimal GastoPecasMesAtual { get; set; }
    public decimal CapitalEstoqueVeiculos { get; set; }
    public decimal ReceitaServicosMesAtual { get; set; }
    public decimal ReceitaVendasMesAtual { get; set; }

    public decimal TotalReceitasMes => ReceitaServicosMesAtual + ReceitaVendasMesAtual;
    public decimal TotalDespesasMes => TotalSalariosMensal + GastoPecasMesAtual;
    public decimal LucroLiquidoMes => TotalReceitasMes - TotalDespesasMes;

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
