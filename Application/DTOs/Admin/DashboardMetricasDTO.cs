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

    public decimal CapitalEstoqueVeiculos { get; set; }

    /// <summary>
    /// Quanto a concessionária pagou (valor de aquisição) pelos veículos ainda
    /// não vendidos — comparado com <see cref="CapitalEstoqueVeiculos"/> (valor
    /// de venda) dá a margem potencial do estoque atual. Não é despesa
    /// recorrente nem entra no fluxo de caixa mensal, é só um retrato do
    /// capital investido, igual a CapitalEstoqueVeiculos.
    /// </summary>
    public decimal CapitalAquisicaoVeiculosDisponiveis { get; set; }

    /// <summary>
    /// Custo unitário × quantidade atual, somado por todos os componentes em
    /// estoque — quanto a oficina tem investido em peças agora. Mesma lógica
    /// de "retrato do capital", não de despesa mensal.
    /// </summary>
    public decimal CapitalEstoqueComponentes { get; set; }

    public decimal ReceitaServicosMesAtual { get; set; }
    public decimal ReceitaVendasMesAtual { get; set; }

    public decimal TotalReceitasMes => ReceitaServicosMesAtual + ReceitaVendasMesAtual;
    public decimal TotalDespesasMes => TotalDespesasFixasMensal;
    public decimal LucroLiquidoMes => TotalReceitasMes - TotalDespesasMes;

    /// <summary>
    /// Lucro operacional da oficina (mês corrente) — receita de serviços menos
    /// despesas exclusivas da oficina. Despesas Gerais (compartilhadas) ficam
    /// só no resumo Geral.
    /// </summary>
    public decimal LucroOficinaMes => ReceitaServicosMesAtual - TotalDespesasOficinaMensal;

    /// <summary>
    /// Lucro operacional da concessionária (mês corrente) — receita de vendas
    /// menos despesas exclusivas da concessionária. CapitalEstoqueVeiculos NÃO
    /// entra: é dinheiro parado em estoque, não saída de caixa do mês.
    /// </summary>
    public decimal LucroConcessionariaMes => ReceitaVendasMesAtual - TotalDespesasConcessionariaMensal;

    // === Distribuições para gráficos ===
    public Dictionary<string, int> OrdensServicoPorStatus { get; set; } = new();
    public Dictionary<string, int> VeiculosPorStatus { get; set; } = new();

    public List<MesValorDTO> SerieReceitaServicos { get; set; } = new();
    public List<MesValorDTO> SerieReceitaVendas { get; set; } = new();

    /// <summary>
    /// Despesa por ponto da série (mesmos rótulos de <see cref="SerieReceitaServicos"/> —
    /// dia ou mês, conforme a granularidade escolhida pelo período). Quando a
    /// granularidade é diária, cada ponto é o total REAL do mês daquele dia
    /// (balanço salvo, ou estimativa do modelo se não houver balanço)
    /// dividido pelos dias do mês — nunca o total do mês inteiro repetido em
    /// cada dia (ver docs/redesign/16-granularidade-despesas.md). Quando a
    /// granularidade é mensal, cada ponto é o valor cheio daquele mês, sem
    /// divisão nenhuma.
    /// </summary>
    public List<MesValorDTO> SerieDespesas { get; set; } = new();
    public List<MesValorDTO> SerieDespesasOficina { get; set; } = new();
    public List<MesValorDTO> SerieDespesasConcessionaria { get; set; } = new();

    /// <summary>Receita de serviços acumulada mês a mês, últimos 12 meses.</summary>
    public List<MesValorDTO> SerieReceitaServicosAcumulada12m { get; set; } = new();

    /// <summary>Top 5 mecânicos por receita gerada em OS finalizadas.</summary>
    public List<ReceitaMecanicoDTO> ReceitaPorMecanico { get; set; } = new();

    /// <summary>Top 5 marcas por quantidade de veículos vendidos.</summary>
    public List<VendaMarcaDTO> VendasPorMarca { get; set; } = new();

    /// <summary>Propostas aprovadas vs rejeitadas, últimos 6 meses.</summary>
    public List<PropostaTimelineDTO> PropostasTimeline { get; set; } = new();

    /// <summary>
    /// Catálogo modular de gráficos comparativos — cada entrada já vem pronta
    /// para render (rótulo + valor por categoria). A tela de dashboard usa isso
    /// para popular o seletor da aba Geral e os cartões fixos das abas
    /// Oficina/Concessionária, sem precisar de um campo dedicado por gráfico.
    /// </summary>
    public List<GraficoAnaliseDTO> Graficos { get; set; } = new();
}

/// <summary>Um gráfico comparativo pronto para render — id estável + dados já agregados.</summary>
public class GraficoAnaliseDTO
{
    public string Id { get; set; } = "";
    public string Titulo { get; set; } = "";

    /// <summary>Agrupamento usado pelo seletor da aba Geral: "Financeiro", "Pessoas", "Concessionária", "Oficina", "Estoque".</summary>
    public string Categoria { get; set; } = "";

    public List<CategoriaValorDTO> Dados { get; set; } = new();
}

public class CategoriaValorDTO
{
    public string Rotulo { get; set; } = "";
    public decimal Valor { get; set; }
}

public class MesValorDTO
{
    /// <summary>Rótulo do ponto — "dd/MM" quando agrupado por dia, "MMM/yy" quando por mês (ver <see cref="CarStoreManager.Application.Services.Admin.DashboardService"/>).</summary>
    public string Label { get; set; } = "";
    public decimal Valor { get; set; }
}

public class ReceitaMecanicoDTO
{
    public string MecanicoNome { get; set; } = "";
    public decimal Receita { get; set; }
}

public class VendaMarcaDTO
{
    public string Marca { get; set; } = "";
    public int Quantidade { get; set; }
}

public class PropostaTimelineDTO
{
    /// <summary>Rótulo do ponto — "dd/MM" quando agrupado por dia, "MMM/yy" quando por mês.</summary>
    public string Label { get; set; } = "";
    public int Aprovadas { get; set; }
    public int Rejeitadas { get; set; }
}
