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
    public decimal DespesasFixasGeralMensal { get; set; }
    public decimal DespesasFixasOficinaMensal { get; set; }
    public decimal DespesasFixasConcessionariaMensal { get; set; }

    // Totalizadores derivados — facilitam o relatório
    public decimal DespesasFixasTotalMensal
        => DespesasFixasGeralMensal + DespesasFixasOficinaMensal + DespesasFixasConcessionariaMensal;
    public decimal DespesasFixasGeralPeriodo => DespesasFixasGeralMensal * QuantidadeMeses;
    public decimal DespesasFixasOficinaPeriodo => DespesasFixasOficinaMensal * QuantidadeMeses;
    public decimal DespesasFixasConcessionariaPeriodo => DespesasFixasConcessionariaMensal * QuantidadeMeses;
    public decimal DespesasFixasTotalPeriodo => DespesasFixasTotalMensal * QuantidadeMeses;

    public decimal TotalReceitas => ReceitaServicos + ReceitaVendas;
    public decimal TotalDespesas => GastoPecas + CompraMaterial + DespesasFixasTotalPeriodo;
    public decimal LucroLiquido => TotalReceitas - TotalDespesas;

    // Lucros operacionais por setor (mesma regra do dashboard, mas com período)
    public decimal LucroOficina
        => ReceitaServicos - GastoPecas - DespesasFixasOficinaPeriodo;
    public decimal LucroConcessionaria
        => ReceitaVendas - CompraMaterial - DespesasFixasConcessionariaPeriodo;

    // Capital imobilizado é snapshot atual (não tem como recompor data-data)
    public decimal CapitalEstoqueVeiculos { get; set; }

    // Evolução mês a mês dentro do período — facilita ver tendência
    public List<MesValorDTO> SerieReceitaServicos { get; set; } = new();
    public List<MesValorDTO> SerieReceitaVendas { get; set; } = new();
    public List<MesValorDTO> SerieGastoPecas { get; set; } = new();
    public List<MesValorDTO> SerieCompraMaterial { get; set; } = new();
}
