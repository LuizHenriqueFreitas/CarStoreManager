namespace CarStoreManager.Application.DTOs.Admin;

/// <summary>
/// Consolidado financeiro para um período arbitrário escolhido pelo admin
/// (≠ DashboardMetricasDTO, que é sempre "mês atual + 6 meses fixos").
///
/// As despesas fixas mensais são multiplicadas pela <see cref="QuantidadeMeses"/>
/// que o período toca — uma despesa de R$ 1.000/mês em um período de
/// 3 meses entra como R$ 3.000 no fluxo.
/// </summary>
public class FluxoCaixaPeriodoDTO
{
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }

    /// <summary>Quantidade de meses-calendário que o período toca (mínimo 1).</summary>
    public int QuantidadeMeses { get; set; }

    // === Movimentos efetivos no período ===
    public decimal ReceitaServicos { get; set; }
    public decimal ReceitaVendas { get; set; }
    public decimal GastoPecas { get; set; }

    /// <summary>
    /// Compra de material da concessionária — soma do custo de aquisição dos
    /// veículos cadastrados dentro do período (análogo ao GastoPecas).
    /// </summary>
    public decimal CompraMaterial { get; set; }

    // === Despesas fixas recorrentes (valor mensal cadastrado) ===
    /// <summary>Soma mensal de todas as despesas fixas ativas.</summary>
    public decimal DespesasFixasTotalMensal { get; set; }

    /// <summary>Despesas fixas mensais por tipo (nome do tipo → valor mensal).</summary>
    public Dictionary<string, decimal> DespesasFixasPorTipoMensal { get; set; } = new();

    // Totalizadores derivados — facilitam o relatório
    public decimal DespesasFixasTotalPeriodo => DespesasFixasTotalMensal * QuantidadeMeses;

    public decimal TotalReceitas => ReceitaServicos + ReceitaVendas;
    public decimal TotalDespesas => GastoPecas + CompraMaterial + DespesasFixasTotalPeriodo;
    public decimal LucroLiquido => TotalReceitas - TotalDespesas;

    // Lucros operacionais por setor — custos diretos do setor (sem rateio das
    // despesas fixas, que agora são classificadas por tipo, não por setor).
    public decimal LucroOficina => ReceitaServicos - GastoPecas;
    public decimal LucroConcessionaria => ReceitaVendas - CompraMaterial;

    // Capital imobilizado é snapshot atual (não tem como recompor data-data)
    public decimal CapitalEstoqueVeiculos { get; set; }

    // Evolução mês a mês dentro do período — facilita ver tendência
    public List<MesValorDTO> SerieReceitaServicos { get; set; } = new();
    public List<MesValorDTO> SerieReceitaVendas { get; set; } = new();
    public List<MesValorDTO> SerieGastoPecas { get; set; } = new();
    public List<MesValorDTO> SerieCompraMaterial { get; set; } = new();
}
