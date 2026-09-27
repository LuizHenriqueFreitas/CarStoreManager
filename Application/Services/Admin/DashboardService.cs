using System.Globalization;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Application.Services.Dashboards;

/// <summary>
/// Calcula métricas agregadas para o dashboard administrativo.
/// Faz tudo em memória — adequado para volume de uma loja pequena/média.
/// Para escala maior, mover para queries SQL agregadas.
/// </summary>
public class DashboardService : IDashboardService
{
    private const int JANELA_MESES_PADRAO = 6;
    private static readonly int[] JANELAS_MESES_PERMITIDAS = { 3, 6, 12 };
    private const int JANELA_MESES_ACUMULADA = 12;
    // Ver docs/redesign/14-granularidade-diaria-graficos.md — períodos de até
    // ~2 meses agrupam por dia (senão um período de 7/15 dias, o padrão do
    // SeletorPeriodo, virava 1 ponto só de gráfico); acima disso, por mês.
    private const int LIMITE_DIAS_GRANULARIDADE_DIARIA = 62;
    // Teto de segurança pro payload — a decisão final de quantas categorias
    // aparecem (pizza com todas se <=6, ou barra com só as 10 maiores) é do
    // JS (carstoreChart.comparativo, Web/wwwroot/js/charts.js), não daqui.
    private const int TOP_CATEGORIAS = 15;

    /// <summary>
    /// OS com trabalho técnico terminado E já pago — <see cref="StatusOrdemServico.Finalizada"/>
    /// é esse estado logo após o pagamento (aguardando retirada);
    /// <see cref="StatusOrdemServico.Entregue"/> é a MESMA OS depois que o
    /// cliente retirou o carro — ainda paga, só avançou de status. Filtrar
    /// só por "Finalizada" (como o código fazia antes) exclui toda OS já
    /// entregue da receita — na prática, a maioria das OS pagas, já que
    /// "Finalizada" é só uma parada de passagem rumo a "Entregue". Ver
    /// docs/redesign/19-bug-receita-os-entregue.md.
    /// </summary>
    private static bool EhServicoConcluidoEPago(StatusOrdemServico status)
        => status == StatusOrdemServico.Finalizada || status == StatusOrdemServico.Entregue;

    private readonly IDespesaRepository _despesas;
    private readonly IBalancoMensalDespesaRepository _balancos;
    private readonly IClienteRepository _clientes;
    private readonly IOrdemServicoRepository _ordens;
    private readonly IPropostaVendaRepository _propostas;
    private readonly IVeiculoVendaRepository _veiculos;
    private readonly IMecanicoRepository _mecanicos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IVeiculoConsignacaoRepository _consignacoes;
    private readonly IVeiculoClienteRepository _veiculosCliente;
    private readonly IComponenteRepository _componentes;
    private readonly IEstoqueRepository _estoque;
    private readonly IVendaMercadoLivreRepository _vendasMercadoLivre;
    private readonly IConfiguracaoSistemaService _configuracaoService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDespesaRepository despesas,
        IBalancoMensalDespesaRepository balancos,
        IClienteRepository clientes,
        IOrdemServicoRepository ordens,
        IPropostaVendaRepository propostas,
        IVeiculoVendaRepository veiculos,
        IMecanicoRepository mecanicos,
        IUsuarioRepository usuarios,
        IVeiculoConsignacaoRepository consignacoes,
        IVeiculoClienteRepository veiculosCliente,
        IComponenteRepository componentes,
        IEstoqueRepository estoque,
        IVendaMercadoLivreRepository vendasMercadoLivre,
        IConfiguracaoSistemaService configuracaoService,
        ILogger<DashboardService> logger)
    {
        _despesas = despesas;
        _balancos = balancos;
        _clientes = clientes;
        _ordens = ordens;
        _propostas = propostas;
        _veiculos = veiculos;
        _mecanicos = mecanicos;
        _usuarios = usuarios;
        _consignacoes = consignacoes;
        _veiculosCliente = veiculosCliente;
        _componentes = componentes;
        _estoque = estoque;
        _vendasMercadoLivre = vendasMercadoLivre;
        _configuracaoService = configuracaoService;
        _logger = logger;
    }

    /// <summary>
    /// Zera a contribuição de um setor com módulo desativado — só nos
    /// agregados COMBINADOS (a fonte de verdade no banco nunca é tocada,
    /// isso é só o DTO de leitura). Chamado antes de qualquer estrutura
    /// derivada (gráficos, cards) ser montada a partir do dto.
    /// </summary>
    private async Task AplicarModulosAtivosAsync(DashboardMetricasDTO dto)
    {
        var r = await _configuracaoService.ObterModulosAtivosAsync();
        if (!r.IsSuccess) return;

        if (!r.Value.Oficina)
        {
            dto.TotalDespesasFixasMensal -= dto.TotalDespesasOficinaMensal;
            dto.ReceitaServicosMesAtual = 0;
            dto.TotalDespesasOficinaMensal = 0;
        }
        if (!r.Value.Concessionaria)
        {
            dto.TotalDespesasFixasMensal -= dto.TotalDespesasConcessionariaMensal;
            dto.ReceitaVendasMesAtual = 0;
            dto.TotalDespesasConcessionariaMensal = 0;
        }
    }

    /// <summary>
    /// Métricas do mês atual — a agregação de verdade fica em ObterMetricasInternoAsync;
    /// aqui só blinda contra exceção inesperada em qualquer um dos ~10 repositórios agregados.
    /// </summary>
    /// <param name="meses">
    /// Janela (em meses) usada pelas séries temporais dos gráficos (receitas/despesas,
    /// comparativo oficina x concessionária, timeline de propostas). Aceita 3, 6 ou 12 —
    /// qualquer outro valor cai no padrão de 6.
    /// </param>
    public async Task<Result<DashboardMetricasDTO>> ObterMetricasAsync(int meses = JANELA_MESES_PADRAO)
    {
        var janela = JANELAS_MESES_PERMITIDAS.Contains(meses) ? meses : JANELA_MESES_PADRAO;
        try
        {
            return await ObterMetricasInternoAsync(janela);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao calcular métricas do dashboard");
            return Result<DashboardMetricasDTO>.Fail("Não foi possível carregar as métricas do dashboard. Tente novamente em instantes.");
        }
    }

    private async Task<Result<DashboardMetricasDTO>> ObterMetricasInternoAsync(int janelaMeses)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var janelaInicio = inicioMes.AddMonths(-(janelaMeses - 1));

        var dto = new DashboardMetricasDTO();

        // === Despesas do mês ===
        // Se já existe um balanço mensal para a competência corrente, usa os
        // valores reais lançados nele. Senão, cai no formulário-modelo (soma das
        // linhas ativas) como estimativa. Ver docs/redesign/11-batch2-melhorias.md §G.
        var despesasAtivas = (await _despesas.GetAtivasAsync()).ToList();
        var balancoMes = await _balancos.ObterPorCompetenciaAsync(new DateOnly(hoje.Year, hoje.Month, 1));
        if (balancoMes is not null)
        {
            dto.TotalDespesasFixasMensal = balancoMes.Total();
            dto.TotalDespesasGeralMensal = balancoMes.TotalPorSetor(SetorDespesa.Geral);
            dto.TotalDespesasOficinaMensal = balancoMes.TotalPorSetor(SetorDespesa.Oficina);
            dto.TotalDespesasConcessionariaMensal = balancoMes.TotalPorSetor(SetorDespesa.Concessionaria);
        }
        else
        {
            dto.TotalDespesasFixasMensal = despesasAtivas.Sum(d => d.GetValor());
            dto.TotalDespesasGeralMensal = despesasAtivas.Where(d => d.Setor == SetorDespesa.Geral).Sum(d => d.GetValor());
            dto.TotalDespesasOficinaMensal = despesasAtivas.Where(d => d.Setor == SetorDespesa.Oficina).Sum(d => d.GetValor());
            dto.TotalDespesasConcessionariaMensal = despesasAtivas.Where(d => d.Setor == SetorDespesa.Concessionaria).Sum(d => d.GetValor());
        }

        // === Receita de serviços (OS finalizadas) ===
        var todasOrdens = (await _ordens.GetAllAsync()).ToList();

        dto.OrdensServicoPorStatus = todasOrdens
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var ordensFinalizadas = todasOrdens
            .Where(o => EhServicoConcluidoEPago(o.Status))
            .ToList();

        dto.ReceitaServicosMesAtual = ordensFinalizadas
            .Where(o => o.DataCriacao >= inicioMes)
            .Sum(o => o.GetValorTotal());

        dto.SerieReceitaServicos = AgruparPorMes(
            ordensFinalizadas
                .Where(o => o.DataCriacao >= janelaInicio)
                .Select(o => (Data: o.DataCriacao, Valor: o.GetValorTotal())),
            janelaInicio, janelaMeses);

        // === Receita de serviços acumulada (12 meses) ===
        var janelaInicio12m = inicioMes.AddMonths(-(JANELA_MESES_ACUMULADA - 1));
        var serie12m = AgruparPorMes(
            ordensFinalizadas
                .Where(o => o.DataCriacao >= janelaInicio12m)
                .Select(o => (Data: o.DataCriacao, Valor: o.GetValorTotal())),
            janelaInicio12m,
            JANELA_MESES_ACUMULADA);

        decimal acumulado = 0;
        dto.SerieReceitaServicosAcumulada12m = serie12m
            .Select(s => new MesValorDTO { Label = s.Label, Valor = acumulado += s.Valor })
            .ToList();

        // === Receita por mecânico (top 5, OS finalizadas) ===
        var mecanicosLista = (await _mecanicos.GetAllAsync()).ToList();
        var mecanicosPorId = mecanicosLista.ToDictionary(m => m.Id, m => m.Nome);
        dto.ReceitaPorMecanico = ordensFinalizadas
            .GroupBy(o => o.MecanicoId)
            .Select(g => new ReceitaMecanicoDTO
            {
                MecanicoNome = mecanicosPorId.TryGetValue(g.Key, out var nome) ? nome : "Desconhecido",
                Receita = g.Sum(o => o.GetValorTotal())
            })
            .OrderByDescending(x => x.Receita)
            .Take(5)
            .ToList();

        // === Receita de vendas de veículos (PropostaVenda concluída) ===
        var todasPropostas = (await _propostas.GetAllAsync()).ToList();

        var propostasFechadas = todasPropostas
            .Where(p => p.Status == StatusPropostaVenda.Concluida && p.DataAprovacao.HasValue)
            .ToList();

        dto.ReceitaVendasMesAtual = propostasFechadas
            .Where(p => p.DataAprovacao!.Value >= inicioMes)
            .Sum(p => p.GetValorFinal());

        dto.SerieReceitaVendas = AgruparPorMes(
            propostasFechadas
                .Where(p => p.DataAprovacao!.Value >= janelaInicio)
                .Select(p => (Data: p.DataAprovacao!.Value, Valor: p.GetValorFinal())),
            janelaInicio, janelaMeses);

        await AplicarModulosAtivosAsync(dto);

        // === Propostas aprovadas vs rejeitadas (janela selecionada) ===
        // "Aprovadas" conta qualquer proposta que já passou pelo status Aprovada
        // (DataAprovacao preenchida), mesmo que tenha avançado no fluxo depois.
        // PropostaVenda não guarda data de rejeição, então rejeitadas são
        // agrupadas pela data de criação.
        var aprovadasPorMes = todasPropostas
            .Where(p => p.DataAprovacao.HasValue && p.DataAprovacao.Value >= janelaInicio)
            .GroupBy(p => new { p.DataAprovacao!.Value.Year, p.DataAprovacao.Value.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

        var rejeitadasPorMes = todasPropostas
            .Where(p => p.Status == StatusPropostaVenda.Rejeitada && p.DataCriacao >= janelaInicio)
            .GroupBy(p => new { p.DataCriacao.Year, p.DataCriacao.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

        var ciProp = CultureInfo.GetCultureInfo("pt-BR");
        var timeline = new List<PropostaTimelineDTO>();
        for (int i = 0; i < janelaMeses; i++)
        {
            var mes = janelaInicio.AddMonths(i);
            var chave = (mes.Year, mes.Month);
            timeline.Add(new PropostaTimelineDTO
            {
                Label = $"{ciProp.DateTimeFormat.AbbreviatedMonthNames[mes.Month - 1]}/{mes.Year % 100:D2}",
                Aprovadas = aprovadasPorMes.TryGetValue(chave, out var a) ? a : 0,
                Rejeitadas = rejeitadasPorMes.TryGetValue(chave, out var r) ? r : 0
            });
        }
        dto.PropostasTimeline = timeline;

        // === Capital imobilizado em veículos disponíveis ===
        var veiculos = (await _veiculos.GetAllAsync()).ToList();
        dto.CapitalEstoqueVeiculos = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.Valor.GetValorDinheiro());
        dto.CapitalAquisicaoVeiculosDisponiveis = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.ValorAquisicao.GetValorDinheiro());

        // === Veículos por status ===
        dto.VeiculosPorStatus = veiculos
            .GroupBy(v => v.Disponibilidade.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // === Vendas por marca (top 5, veículos vendidos — mantido por compatibilidade) ===
        dto.VendasPorMarca = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Vendido)
            .GroupBy(v => v.Marca)
            .Select(g => new VendaMarcaDTO { Marca = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        // === Dados extra para o catálogo modular de gráficos comparativos ===
        var todosUsuarios = (await _usuarios.GetAllAsync()).ToList();
        var todasConsignacoes = (await _consignacoes.GetAllAsync()).ToList();
        var todosVeiculosCliente = (await _veiculosCliente.GetAllAsync()).ToList();
        var veiculosClientePorId = todosVeiculosCliente.ToDictionary(v => v.Id);
        var todosComponentes = (await _componentes.GetAllAsync()).ToList();
        var componentesPorId = todosComponentes.ToDictionary(c => c.Id);
        var todoEstoque = (await _estoque.GetAllAsync()).ToList();
        var todasVendasML = (await _vendasMercadoLivre.GetAllAsync()).ToList();
        var todosClientes = (await _clientes.GetAllAsync()).ToList();
        var clientesPorId = todosClientes.ToDictionary(c => c.Id, c => c.Nome);

        dto.CapitalEstoqueComponentes = todoEstoque
            .Where(e => e.Componente is not null)
            .Sum(e => e.Componente.CustoUnitario * e.QuantidadeAtual);

        dto.Graficos = MontarGraficos(
            despesasAtivas, todasOrdens, todasPropostas, propostasFechadas, veiculos,
            todosUsuarios, todasConsignacoes, todosVeiculosCliente, veiculosClientePorId,
            componentesPorId, todoEstoque, todasVendasML, mecanicosLista, clientesPorId);

        // Composição financeira do mês — inserida na frente da lista para ser
        // a opção padrão do seletor ("dados financeiros básicos por padrão").
        dto.Graficos.Insert(0, new GraficoAnaliseDTO
        {
            Id = "financeiro-composicao",
            Titulo = "Composição financeira do mês",
            Categoria = "Financeiro",
            Dados = new List<CategoriaValorDTO>
            {
                new() { Rotulo = "Receita serviços", Valor = dto.ReceitaServicosMesAtual },
                new() { Rotulo = "Receita vendas", Valor = dto.ReceitaVendasMesAtual },
                new() { Rotulo = "Despesas Geral", Valor = dto.TotalDespesasGeralMensal },
                new() { Rotulo = "Despesas Oficina", Valor = dto.TotalDespesasOficinaMensal },
                new() { Rotulo = "Despesas Concessionária", Valor = dto.TotalDespesasConcessionariaMensal }
            }.Where(c => c.Valor > 0).ToList()
        });

        return Result<DashboardMetricasDTO>.Ok(dto);
    }

    /// <summary>Mesma ideia de ObterMetricasAsync, mas recalculada pro período [dataInicio, dataFim] — usada pelos relatórios exportáveis.</summary>
    public async Task<Result<DashboardMetricasDTO>> ObterMetricasPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        if (dataFim.Date < dataInicio.Date)
            return Result<DashboardMetricasDTO>.Fail("Data final não pode ser anterior à data inicial.");

        try
        {
            return await ObterMetricasPeriodoInternoAsync(dataInicio, dataFim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao calcular métricas do dashboard para o período {Inicio:yyyy-MM-dd} a {Fim:yyyy-MM-dd}", dataInicio, dataFim);
            return Result<DashboardMetricasDTO>.Fail("Não foi possível carregar as métricas do período. Tente novamente em instantes.");
        }
    }

    private async Task<Result<DashboardMetricasDTO>> ObterMetricasPeriodoInternoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var inicioPeriodo = dataInicio.Date;
        var fimPeriodo = dataFim.Date.AddDays(1).AddTicks(-1); // fim do dia, inclusive

        var dto = new DashboardMetricasDTO();

        // === Despesas — para cada mês tocado pelo período, usa o balanço
        // MENSAL REAL daquele mês se ele existir (histórico de verdade, com
        // as variações — compra de veículo, manutenção extra, mês mais
        // enxuto etc.); só cai pra ESTIMATIVA (valor mensal recorrente
        // cadastrado hoje) nos meses que nunca tiveram um balanço fechado —
        // antes disso era sempre a estimativa, mesmo pra meses com balanço
        // real já registrado, escondendo qualquer variação histórica. ===
        var despesasAtivas = (await _despesas.GetAtivasAsync()).ToList();
        var somaModeloMensal = despesasAtivas.Sum(d => d.GetValor());
        var somaModeloGeral = despesasAtivas.Where(d => d.Setor == SetorDespesa.Geral).Sum(d => d.GetValor());
        var somaModeloOficina = despesasAtivas.Where(d => d.Setor == SetorDespesa.Oficina).Sum(d => d.GetValor());
        var somaModeloConcessionaria = despesasAtivas.Where(d => d.Setor == SetorDespesa.Concessionaria).Sum(d => d.GetValor());

        // Série por ponto (dia ou mês, mesma granularidade da receita) —
        // ver docs/redesign/16-granularidade-despesas.md. Quando a
        // granularidade é diária, cada dia recebe uma fatia pro-rateada do
        // mês a que pertence — os KPIs escalares abaixo SOMAM essa mesma
        // série (em vez de somar o mês inteiro de cada competência tocada,
        // como um código anterior fazia) pra não contar um mês inteiro de
        // despesa quando o período só toca uma fração dele. Isso era a
        // causa raiz de uma janela de 30 dias que cruza virada de mês
        // aparecer com margem de -140%+: um período de 22/08 a 20/09 somava
        // AGOSTO INTEIRO + SETEMBRO INTEIRO de despesa (2 meses cheios,
        // incluindo compras de veículo de dias fora da janela), contra
        // receita de só os ~29 dias reais — ver docs/redesign/19-bug-
        // margem-mensal-negativa.md.
        (dto.SerieDespesas, dto.SerieDespesasOficina, dto.SerieDespesasConcessionaria) =
            await ConstruirSeriesDespesaAsync(inicioPeriodo, fimPeriodo, somaModeloMensal, somaModeloOficina, somaModeloConcessionaria);

        dto.TotalDespesasOficinaMensal = dto.SerieDespesasOficina.Sum(s => s.Valor);
        dto.TotalDespesasConcessionariaMensal = dto.SerieDespesasConcessionaria.Sum(s => s.Valor);
        dto.TotalDespesasFixasMensal = dto.SerieDespesas.Sum(s => s.Valor);
        dto.TotalDespesasGeralMensal = dto.TotalDespesasFixasMensal - dto.TotalDespesasOficinaMensal - dto.TotalDespesasConcessionariaMensal;

        // === Ordens de serviço criadas no período ===
        var ordensNoPeriodo = (await _ordens.GetAllAsync())
            .Where(o => o.DataCriacao >= inicioPeriodo && o.DataCriacao <= fimPeriodo)
            .ToList();

        dto.OrdensServicoPorStatus = ordensNoPeriodo
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var ordensFinalizadas = ordensNoPeriodo
            .Where(o => EhServicoConcluidoEPago(o.Status))
            .ToList();

        dto.ReceitaServicosMesAtual = ordensFinalizadas.Sum(o => o.GetValorTotal());

        dto.SerieReceitaServicos = AgruparPorPeriodo(
            ordensFinalizadas.Select(o => (Data: o.DataCriacao, Valor: o.GetValorTotal())),
            inicioPeriodo, fimPeriodo);

        decimal acumulado = 0;
        dto.SerieReceitaServicosAcumulada12m = dto.SerieReceitaServicos
            .Select(s => new MesValorDTO { Label = s.Label, Valor = acumulado += s.Valor })
            .ToList();

        var mecanicosPorId = (await _mecanicos.GetAllAsync()).ToDictionary(m => m.Id, m => m.Nome);
        dto.ReceitaPorMecanico = ordensFinalizadas
            .GroupBy(o => o.MecanicoId)
            .Select(g => new ReceitaMecanicoDTO
            {
                MecanicoNome = mecanicosPorId.TryGetValue(g.Key, out var nome) ? nome : "Desconhecido",
                Receita = g.Sum(o => o.GetValorTotal())
            })
            .OrderByDescending(x => x.Receita)
            .Take(5)
            .ToList();

        // === Vendas de veículos (propostas concluídas, pela data de aprovação) no período ===
        var todasPropostas = (await _propostas.GetAllAsync()).ToList();
        var propostasFechadasPeriodo = todasPropostas
            .Where(p => p.Status == StatusPropostaVenda.Concluida && p.DataAprovacao.HasValue
                && p.DataAprovacao.Value >= inicioPeriodo && p.DataAprovacao.Value <= fimPeriodo)
            .ToList();

        dto.ReceitaVendasMesAtual = propostasFechadasPeriodo.Sum(p => p.GetValorFinal());

        dto.SerieReceitaVendas = AgruparPorPeriodo(
            propostasFechadasPeriodo.Select(p => (Data: p.DataAprovacao!.Value, Valor: p.GetValorFinal())),
            inicioPeriodo, fimPeriodo);

        await AplicarModulosAtivosAsync(dto);

        // === Propostas aprovadas vs rejeitadas dentro do período — por dia se
        // o período for curto (ver LIMITE_DIAS_GRANULARIDADE_DIARIA / doc 14),
        // por mês senão. ===
        var timeline = new List<PropostaTimelineDTO>();
        var diasPeriodoPropostas = (fimPeriodo.Date - inicioPeriodo.Date).Days + 1;

        if (diasPeriodoPropostas <= LIMITE_DIAS_GRANULARIDADE_DIARIA)
        {
            var aprovadasPorDia = todasPropostas
                .Where(p => p.DataAprovacao.HasValue && p.DataAprovacao.Value >= inicioPeriodo && p.DataAprovacao.Value <= fimPeriodo)
                .GroupBy(p => p.DataAprovacao!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var rejeitadasPorDia = todasPropostas
                .Where(p => p.Status == StatusPropostaVenda.Rejeitada && p.DataCriacao >= inicioPeriodo && p.DataCriacao <= fimPeriodo)
                .GroupBy(p => p.DataCriacao.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            for (int i = 0; i < diasPeriodoPropostas; i++)
            {
                var dia = inicioPeriodo.Date.AddDays(i);
                timeline.Add(new PropostaTimelineDTO
                {
                    Label = dia.ToString("dd/MM"),
                    Aprovadas = aprovadasPorDia.TryGetValue(dia, out var a) ? a : 0,
                    Rejeitadas = rejeitadasPorDia.TryGetValue(dia, out var r) ? r : 0
                });
            }
        }
        else
        {
            var aprovadasPorMes = todasPropostas
                .Where(p => p.DataAprovacao.HasValue && p.DataAprovacao.Value >= inicioPeriodo && p.DataAprovacao.Value <= fimPeriodo)
                .GroupBy(p => new { p.DataAprovacao!.Value.Year, p.DataAprovacao.Value.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

            var rejeitadasPorMes = todasPropostas
                .Where(p => p.Status == StatusPropostaVenda.Rejeitada && p.DataCriacao >= inicioPeriodo && p.DataCriacao <= fimPeriodo)
                .GroupBy(p => new { p.DataCriacao.Year, p.DataCriacao.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

            var ciProp = CultureInfo.GetCultureInfo("pt-BR");
            var cursor = new DateTime(inicioPeriodo.Year, inicioPeriodo.Month, 1);
            var cursorFim = new DateTime(fimPeriodo.Year, fimPeriodo.Month, 1);
            while (cursor <= cursorFim)
            {
                var chave = (cursor.Year, cursor.Month);
                timeline.Add(new PropostaTimelineDTO
                {
                    Label = $"{ciProp.DateTimeFormat.AbbreviatedMonthNames[cursor.Month - 1]}/{cursor.Year % 100:D2}",
                    Aprovadas = aprovadasPorMes.TryGetValue(chave, out var a) ? a : 0,
                    Rejeitadas = rejeitadasPorMes.TryGetValue(chave, out var r) ? r : 0
                });
                cursor = cursor.AddMonths(1);
            }
        }
        dto.PropostasTimeline = timeline;

        // === Veículos — status/capital são um retrato do estoque ATUAL, não
        // fazem sentido recalculados "no passado", então mantêm leitura atual. ===
        var veiculos = (await _veiculos.GetAllAsync()).ToList();
        var veiculosPorId = veiculos.ToDictionary(v => v.Id);
        dto.CapitalEstoqueVeiculos = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.Valor.GetValorDinheiro());
        dto.CapitalAquisicaoVeiculosDisponiveis = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.ValorAquisicao.GetValorDinheiro());
        dto.VeiculosPorStatus = veiculos
            .GroupBy(v => v.Disponibilidade.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var todoEstoque = (await _estoque.GetAllAsync()).ToList();
        dto.CapitalEstoqueComponentes = todoEstoque
            .Where(e => e.Componente is not null)
            .Sum(e => e.Componente.CustoUnitario * e.QuantidadeAtual);

        // === Vendas por marca — veículos das propostas concluídas no período ===
        dto.VendasPorMarca = propostasFechadasPeriodo
            .Where(p => veiculosPorId.ContainsKey(p.VeiculoVendaId))
            .GroupBy(p => veiculosPorId[p.VeiculoVendaId].Marca)
            .Select(g => new VendaMarcaDTO { Marca = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

        // === Catálogo modular de gráficos comparativos ===
        // Mesmo catálogo de ObterMetricasInternoAsync — na prática são
        // "retratos" do estado atual/histórico completo (não recortados pelo
        // período escolhido, igual a "Veículos por status" ou "Top clientes"
        // já eram antes desta tela existir), então busca os dados completos
        // de novo em vez de reaproveitar as variáveis já filtradas acima.
        var todasOrdensCompleto = (await _ordens.GetAllAsync()).ToList();
        var propostasFechadasCompleto = todasPropostas
            .Where(p => p.Status == StatusPropostaVenda.Concluida && p.DataAprovacao.HasValue)
            .ToList();
        var todosUsuarios = (await _usuarios.GetAllAsync()).ToList();
        var todasConsignacoes = (await _consignacoes.GetAllAsync()).ToList();
        var todosVeiculosCliente = (await _veiculosCliente.GetAllAsync()).ToList();
        var veiculosClientePorId = todosVeiculosCliente.ToDictionary(v => v.Id);
        var todosComponentes = (await _componentes.GetAllAsync()).ToList();
        var componentesPorId = todosComponentes.ToDictionary(c => c.Id);
        var todasVendasML = (await _vendasMercadoLivre.GetAllAsync()).ToList();
        var mecanicosLista = (await _mecanicos.GetAllAsync()).ToList();
        var todosClientes = (await _clientes.GetAllAsync()).ToList();
        var clientesPorId = todosClientes.ToDictionary(c => c.Id, c => c.Nome);

        dto.Graficos = MontarGraficos(
            despesasAtivas, todasOrdensCompleto, todasPropostas, propostasFechadasCompleto, veiculos,
            todosUsuarios, todasConsignacoes, todosVeiculosCliente, veiculosClientePorId,
            componentesPorId, todoEstoque, todasVendasML, mecanicosLista, clientesPorId);

        // "financeiro-composicao" usa os totais do PERÍODO escolhido (não do
        // mês corrente) — mesma régua da opção padrão do seletor em
        // ObterMetricasInternoAsync, só que recortada pro intervalo pedido.
        dto.Graficos.Insert(0, new GraficoAnaliseDTO
        {
            Id = "financeiro-composicao",
            Titulo = "Composição financeira do período",
            Categoria = "Financeiro",
            Dados = new List<CategoriaValorDTO>
            {
                new() { Rotulo = "Receita serviços", Valor = dto.ReceitaServicosMesAtual },
                new() { Rotulo = "Receita vendas", Valor = dto.ReceitaVendasMesAtual },
                new() { Rotulo = "Despesas Geral", Valor = dto.TotalDespesasGeralMensal },
                new() { Rotulo = "Despesas Oficina", Valor = dto.TotalDespesasOficinaMensal },
                new() { Rotulo = "Despesas Concessionária", Valor = dto.TotalDespesasConcessionariaMensal }
            }.Where(c => c.Valor > 0).ToList()
        });

        return Result<DashboardMetricasDTO>.Ok(dto);
    }

    /// <summary>
    /// Agrupa lançamentos por mês dentro de [inicio, fim] (inclusive nas duas
    /// pontas), preenchendo meses sem movimento com zero — mesma lógica de
    /// <see cref="AgruparPorMes"/>, mas com número de meses derivado do
    /// período em vez de uma janela fixa.
    /// </summary>
    private static List<MesValorDTO> AgruparPorMesNoPeriodo(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime inicio,
        DateTime fim)
    {
        var mesInicio = new DateTime(inicio.Year, inicio.Month, 1);
        var meses = (fim.Year - inicio.Year) * 12 + fim.Month - inicio.Month + 1;
        return AgruparPorMes(lancamentos, mesInicio, meses);
    }

    /// <summary>
    /// Agrupa lançamentos por DIA dentro de [inicio, inicio + dias - 1],
    /// preenchendo dias sem movimento com zero — mesma lógica de
    /// <see cref="AgruparPorMes"/>, granularidade diária em vez de mensal.
    /// </summary>
    private static List<MesValorDTO> AgruparPorDia(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime inicio,
        int dias)
    {
        var serie = new List<MesValorDTO>();

        var agrupado = lancamentos
            .GroupBy(x => x.Data.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Valor));

        for (int i = 0; i < dias; i++)
        {
            var dia = inicio.Date.AddDays(i);
            var valor = agrupado.TryGetValue(dia, out var v) ? v : 0m;
            serie.Add(new MesValorDTO { Label = dia.ToString("dd/MM"), Valor = valor });
        }
        return serie;
    }

    /// <summary>
    /// Escolhe a granularidade certa pra série temporal de um período livre
    /// (ver docs/redesign/14-granularidade-diaria-graficos.md): períodos
    /// curtos (até <see cref="LIMITE_DIAS_GRANULARIDADE_DIARIA"/> dias) viram
    /// série diária — senão um período de 7/15 dias (padrão do
    /// SeletorPeriodo) caía quase sempre num único mês e virava 1 ponto só de
    /// gráfico; períodos mais longos continuam mensais, senão um intervalo
    /// customizado de meses/anos viraria uma série de centenas de barras.
    /// </summary>
    private static List<MesValorDTO> AgruparPorPeriodo(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime inicio,
        DateTime fim)
    {
        var lista = lancamentos.ToList();
        var dias = (fim.Date - inicio.Date).Days + 1;
        return dias <= LIMITE_DIAS_GRANULARIDADE_DIARIA
            ? AgruparPorDia(lista, inicio, dias)
            : AgruparPorMesNoPeriodo(lista, inicio, fim);
    }

    /// <summary>
    /// Série de despesa (total/oficina/concessionária) com a MESMA
    /// granularidade de <see cref="AgruparPorPeriodo"/> (dia ou mês,
    /// conforme o tamanho do período) — ver
    /// docs/redesign/16-granularidade-despesas.md. Diferente de receita,
    /// despesa não tem lançamento por dia (só por competência/mês), então
    /// granularidade diária aqui significa "fatia pro-rateada do mês",
    /// nunca "o mês inteiro repetido em cada dia".
    /// </summary>
    private async Task<(List<MesValorDTO> Total, List<MesValorDTO> Oficina, List<MesValorDTO> Concessionaria)> ConstruirSeriesDespesaAsync(
        DateTime inicio, DateTime fim,
        decimal somaModeloMensal, decimal somaModeloOficina, decimal somaModeloConcessionaria)
    {
        var dias = (fim.Date - inicio.Date).Days + 1;
        return dias <= LIMITE_DIAS_GRANULARIDADE_DIARIA
            ? await ConstruirSeriesDespesaDiariaAsync(inicio, dias, somaModeloMensal, somaModeloOficina, somaModeloConcessionaria)
            : await ConstruirSeriesDespesaMensalAsync(inicio, fim, somaModeloMensal, somaModeloOficina, somaModeloConcessionaria);
    }

    private async Task<(List<MesValorDTO> Total, List<MesValorDTO> Oficina, List<MesValorDTO> Concessionaria)> ConstruirSeriesDespesaDiariaAsync(
        DateTime inicio, int dias,
        decimal somaModeloMensal, decimal somaModeloOficina, decimal somaModeloConcessionaria)
    {
        var total = new List<MesValorDTO>();
        var oficina = new List<MesValorDTO>();
        var concessionaria = new List<MesValorDTO>();

        // Cache por mês — vários dias do laço caem no mesmo mês, evita
        // reconsultar o mesmo balanço repetidas vezes.
        var cachePorMes = new Dictionary<(int Ano, int Mes), (decimal Total, decimal Oficina, decimal Concessionaria, int DiasNoMes)>();

        for (int i = 0; i < dias; i++)
        {
            var dia = inicio.Date.AddDays(i);
            var chave = (dia.Year, dia.Month);

            if (!cachePorMes.TryGetValue(chave, out var doMes))
            {
                var balancoMes = await _balancos.ObterPorCompetenciaAsync(new DateOnly(dia.Year, dia.Month, 1));
                var diasNoMes = DateTime.DaysInMonth(dia.Year, dia.Month);
                doMes = balancoMes is not null
                    ? (balancoMes.Total(), balancoMes.TotalPorSetor(SetorDespesa.Oficina), balancoMes.TotalPorSetor(SetorDespesa.Concessionaria), diasNoMes)
                    : (somaModeloMensal, somaModeloOficina, somaModeloConcessionaria, diasNoMes);
                cachePorMes[chave] = doMes;
            }

            var label = dia.ToString("dd/MM");
            total.Add(new MesValorDTO { Label = label, Valor = Math.Round(doMes.Total / doMes.DiasNoMes, 2) });
            oficina.Add(new MesValorDTO { Label = label, Valor = Math.Round(doMes.Oficina / doMes.DiasNoMes, 2) });
            concessionaria.Add(new MesValorDTO { Label = label, Valor = Math.Round(doMes.Concessionaria / doMes.DiasNoMes, 2) });
        }

        return (total, oficina, concessionaria);
    }

    private async Task<(List<MesValorDTO> Total, List<MesValorDTO> Oficina, List<MesValorDTO> Concessionaria)> ConstruirSeriesDespesaMensalAsync(
        DateTime inicio, DateTime fim,
        decimal somaModeloMensal, decimal somaModeloOficina, decimal somaModeloConcessionaria)
    {
        var ci = CultureInfo.GetCultureInfo("pt-BR");
        var total = new List<MesValorDTO>();
        var oficina = new List<MesValorDTO>();
        var concessionaria = new List<MesValorDTO>();

        var cursor = new DateOnly(inicio.Year, inicio.Month, 1);
        var ultimoMes = new DateOnly(fim.Year, fim.Month, 1);
        while (cursor <= ultimoMes)
        {
            var balancoMes = await _balancos.ObterPorCompetenciaAsync(cursor);
            var label = $"{ci.DateTimeFormat.AbbreviatedMonthNames[cursor.Month - 1]}/{cursor.Year % 100:D2}";

            if (balancoMes is not null)
            {
                total.Add(new MesValorDTO { Label = label, Valor = balancoMes.Total() });
                oficina.Add(new MesValorDTO { Label = label, Valor = balancoMes.TotalPorSetor(SetorDespesa.Oficina) });
                concessionaria.Add(new MesValorDTO { Label = label, Valor = balancoMes.TotalPorSetor(SetorDespesa.Concessionaria) });
            }
            else
            {
                total.Add(new MesValorDTO { Label = label, Valor = somaModeloMensal });
                oficina.Add(new MesValorDTO { Label = label, Valor = somaModeloOficina });
                concessionaria.Add(new MesValorDTO { Label = label, Valor = somaModeloConcessionaria });
            }
            cursor = cursor.AddMonths(1);
        }

        return (total, oficina, concessionaria);
    }

    /// <summary>
    /// Monta o catálogo modular de gráficos comparativos exibido no seletor da
    /// aba Geral e nos cartões fixos das abas Oficina/Concessionária. Para
    /// adicionar um novo comparativo, basta acrescentar mais uma entrada aqui —
    /// a UI (seletor + render) já é genérica e não precisa de nenhuma alteração.
    /// </summary>
    private List<GraficoAnaliseDTO> MontarGraficos(
        List<Domain.Entities.Sistema.Despesa> despesasAtivas,
        List<Domain.Entities.Oficina.OrdemServico> todasOrdens,
        List<Domain.Entities.Concessionaria.PropostaVenda> todasPropostas,
        List<Domain.Entities.Concessionaria.PropostaVenda> propostasFechadas,
        List<Domain.Entities.Concessionaria.VeiculoVenda> veiculos,
        List<Domain.Entities.Usuario> todosUsuarios,
        List<Domain.Entities.Concessionaria.VeiculoConsignacao> todasConsignacoes,
        List<VeiculoCliente> todosVeiculosCliente,
        Dictionary<Guid, VeiculoCliente> veiculosClientePorId,
        Dictionary<Guid, Componente> componentesPorId,
        List<EstoqueComponente> todoEstoque,
        List<Domain.Entities.Integracoes.VendaMercadoLivre> todasVendasML,
        List<Domain.Entities.Oficina.Mecanico> mecanicosLista,
        Dictionary<Guid, string> clientesPorId)
    {
        var graficos = new List<GraficoAnaliseDTO>();
        var veiculosVendidos = veiculos.Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Vendido).ToList();
        var consignacoesVendidas = todasConsignacoes
            .Where(c => c.Status is StatusConsignacao.Concluida or StatusConsignacao.VendidoAguardandoPagamento)
            .ToList();

        // ===================== FINANCEIRO =====================
        // "financeiro-composicao" (opção padrão do seletor) é montado em
        // ObterMetricasAsync, onde os totais do mês já estão calculados.

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "despesas-por-tipo",
            Titulo = "Despesas por tipo (valor gasto)",
            Categoria = "Financeiro",
            Dados = Top(
                despesasAtivas.GroupBy(d => d.Tipo.ToString())
                    .Select(g => (Rotulo: g.Key, Valor: g.Sum(d => d.GetValor()))))
        });

        var ticketVeiculoLoja = MediaOuZero(propostasFechadas.Select(p => p.GetValorFinal()));
        var ticketOS = MediaOuZero(todasOrdens.Where(o => EhServicoConcluidoEPago(o.Status)).Select(o => o.GetValorTotal()));
        var ticketConsignacao = MediaOuZero(consignacoesVendidas.Select(c => c.Comissao.ValorVendaEsperado.GetValorDinheiro()));
        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "valores-medios",
            Titulo = "Valores médios (ticket)",
            Categoria = "Financeiro",
            Dados = new()
            {
                new() { Rotulo = "Venda de veículo (loja)", Valor = ticketVeiculoLoja },
                new() { Rotulo = "Venda consignada", Valor = ticketConsignacao },
                new() { Rotulo = "Ordem de serviço", Valor = ticketOS }
            }
        });

        // ===================== PESSOAS =====================

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "funcionarios-por-tipo",
            Titulo = "Funcionários cadastrados por tipo",
            Categoria = "Pessoas",
            Dados = todosUsuarios
                .GroupBy(u => RotuloRole(u.GetRole()))
                .Select(g => new CategoriaValorDTO { Rotulo = g.Key, Valor = g.Count() })
                .OrderByDescending(c => c.Valor)
                .ToList()
        });

        // ===================== CONCESSIONÁRIA =====================
        // Marca/modelo/cor/câmbio/combustível "mais vendido" considera vendas
        // via estoque próprio E via consignação — do ponto de vista do
        // negócio ambas são vendas realizadas pela concessionária.

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "marcas-vendidas",
            Titulo = "Marcas mais vendidas",
            Categoria = "Concessionária",
            Dados = Top(ContarPorTexto(
                veiculosVendidos.Select(v => v.Marca).Concat(consignacoesVendidas.Select(c => c.Marca))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "modelos-vendidos",
            Titulo = "Modelos mais vendidos",
            Categoria = "Concessionária",
            Dados = Top(ContarPorTexto(
                veiculosVendidos.Select(v => v.Modelo).Concat(consignacoesVendidas.Select(c => c.Modelo))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "cores-vendidas",
            Titulo = "Cores mais vendidas",
            Categoria = "Concessionária",
            Dados = Top(ContarPorTexto(
                veiculosVendidos.Select(v => v.Cor).Concat(consignacoesVendidas.Select(c => c.Cor))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "combustivel-vendido",
            Titulo = "Combustível mais vendido",
            Categoria = "Concessionária",
            Dados = Top(ContarPorTexto(
                veiculosVendidos.Select(v => v.Combustivel.ToString())
                    .Concat(consignacoesVendidas.Select(c => c.Combustivel.ToString()))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "cambio-vendido",
            Titulo = "Câmbio mais vendido",
            Categoria = "Concessionária",
            Dados = Top(ContarPorTexto(
                veiculosVendidos.Select(v => v.Cambio.ToString())
                    .Concat(consignacoesVendidas.Select(c => c.Cambio.ToString()))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "acessorios-veiculos",
            Titulo = "Acessórios mais comuns no estoque",
            Categoria = "Concessionária",
            Dados = Top(ContarAcessorios(veiculos.Select(v => v.Acessorios)))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "vendas-consignacao-loja",
            Titulo = "Vendas: consignação vs. loja própria",
            Categoria = "Concessionária",
            Dados = new()
            {
                new() { Rotulo = "Loja própria", Valor = veiculosVendidos.Count },
                new() { Rotulo = "Consignação", Valor = consignacoesVendidas.Count }
            }
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "modos-pagamento",
            Titulo = "Modos de pagamento (propostas)",
            Categoria = "Concessionária",
            Dados = Top(
                todasPropostas
                    .Where(p => p.ModoPagamento != ModoPagamento.NaoDefinido)
                    .GroupBy(p => p.ModoPagamento.ToString())
                    .Select(g => (Rotulo: g.Key, Valor: (decimal)g.Count())))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "veiculos-status",
            Titulo = "Veículos por status",
            Categoria = "Concessionária",
            Dados = veiculos
                .GroupBy(v => v.Disponibilidade.ToString())
                .Select(g => new CategoriaValorDTO { Rotulo = g.Key, Valor = g.Count() })
                .ToList()
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "vendas-canal",
            Titulo = "Vendas: e-commerce vs. loja física",
            Categoria = "Concessionária",
            Dados = new()
            {
                new() { Rotulo = "Loja física", Valor = propostasFechadas.Count },
                new() { Rotulo = "E-commerce (Mercado Livre)", Valor = todasVendasML.Count }
            }
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "top-clientes-compradores",
            Titulo = "Top 5 clientes que mais compraram veículos",
            Categoria = "Concessionária",
            Dados = propostasFechadas
                .GroupBy(p => p.ClienteId)
                .Select(g => new CategoriaValorDTO { Rotulo = NomeCliente(clientesPorId, g.Key), Valor = g.Count() })
                .OrderByDescending(c => c.Valor)
                .Take(5)
                .ToList()
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "top-clientes-consignantes",
            Titulo = "Top 5 clientes com mais veículos consignados",
            Categoria = "Concessionária",
            Dados = todasConsignacoes
                .GroupBy(c => c.ClienteProprietarioId)
                .Select(g => new CategoriaValorDTO { Rotulo = NomeCliente(clientesPorId, g.Key), Valor = g.Count() })
                .OrderByDescending(c => c.Valor)
                .Take(5)
                .ToList()
        });

        // ===================== OFICINA =====================

        // "sistemas-consertados" (agrupava OrdemServico.Itens por Componente.Sistema)
        // foi removido: os geradores de dados de demonstração nunca criam OS já
        // com itens/peças (OrdemServico nasce vazia, peças são adicionadas depois
        // pelo mecânico), então esse gráfico sempre ficava vazio. Substituído por
        // um comparativo que sempre tem dado: mecânicos por especialização.
        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "mecanicos-por-especializacao",
            Titulo = "Mecânicos por especialização",
            Categoria = "Oficina",
            Dados = mecanicosLista
                .GroupBy(m => m.GetEspecialidade())
                .Select(g => new CategoriaValorDTO { Rotulo = g.Key, Valor = g.Count() })
                .OrderByDescending(c => c.Valor)
                .ToList()
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "servicos-tipo",
            Titulo = "Tipos de serviço mais realizados",
            Categoria = "Oficina",
            Dados = todasOrdens
                .GroupBy(o => o.Tipo.ToString())
                .Select(g => new CategoriaValorDTO { Rotulo = g.Key, Valor = g.Count() })
                .OrderByDescending(c => c.Valor)
                .ToList()
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "os-status",
            Titulo = "Ordens de serviço por status",
            Categoria = "Oficina",
            Dados = todasOrdens
                .GroupBy(o => o.Status.ToString())
                .Select(g => new CategoriaValorDTO { Rotulo = g.Key, Valor = g.Count() })
                .ToList()
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "marcas-visitam-oficina",
            Titulo = "Marcas que mais visitam a oficina",
            Categoria = "Oficina",
            Dados = Top(ContarPorTexto(
                todasOrdens
                    .Where(o => veiculosClientePorId.ContainsKey(o.VeiculoClienteId))
                    .Select(o => veiculosClientePorId[o.VeiculoClienteId].Marca)))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "modelos-visitam-oficina",
            Titulo = "Modelos que mais visitam a oficina",
            Categoria = "Oficina",
            Dados = Top(ContarPorTexto(
                todasOrdens
                    .Where(o => veiculosClientePorId.ContainsKey(o.VeiculoClienteId))
                    .Select(o => veiculosClientePorId[o.VeiculoClienteId].Modelo)))
        });

        // Não há timestamp de conclusão real na OS (só criação + prazo
        // estimado) — este comparativo usa o prazo estimado como proxy do
        // tempo de execução planejado, não o tempo real decorrido.
        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "prazo-medio-servico",
            Titulo = "Prazo médio estimado por tipo de serviço (dias)",
            Categoria = "Oficina",
            Dados = todasOrdens
                .GroupBy(o => o.Tipo.ToString())
                .Select(g => new CategoriaValorDTO
                {
                    Rotulo = g.Key,
                    Valor = Math.Round((decimal)g.Average(o => (o.PrazoEstimado - o.DataCriacao).TotalDays), 1)
                })
                .OrderByDescending(c => c.Valor)
                .ToList()
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "top-clientes-oficina",
            Titulo = "Top 5 clientes que mais levaram veículos à oficina",
            Categoria = "Oficina",
            Dados = todasOrdens
                .GroupBy(o => o.ClienteId)
                .Select(g => new CategoriaValorDTO { Rotulo = NomeCliente(clientesPorId, g.Key), Valor = g.Count() })
                .OrderByDescending(c => c.Valor)
                .Take(5)
                .ToList()
        });

        // ===================== ESTOQUE =====================

        var estoquePorSistema = todoEstoque
            .Where(e => e.Componente is not null && e.Componente.Sistema.HasValue)
            .GroupBy(e => e.Componente.Sistema!.Value.ToString())
            .Select(g => (Rotulo: g.Key, Valor: (decimal)g.Sum(e => e.QuantidadeAtual)));
        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "estoque-por-sistema",
            Titulo = "Componentes em estoque por sistema (quantidade)",
            Categoria = "Estoque",
            Dados = Top(estoquePorSistema)
        });

        return graficos;
    }

    private static decimal MediaOuZero(IEnumerable<decimal> valores)
    {
        var lista = valores.ToList();
        return lista.Count == 0 ? 0m : Math.Round(lista.Average(), 2);
    }

    private static IEnumerable<(string Rotulo, decimal Valor)> ContarPorTexto(IEnumerable<string> valores)
        => valores
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .GroupBy(v => v.Trim())
            .Select(g => (Rotulo: g.Key, Valor: (decimal)g.Count()));

    private static IEnumerable<(string Rotulo, decimal Valor)> ContarAcessorios(IEnumerable<AcessoriosVeiculo> valores)
    {
        var lista = valores.ToList();
        return Enum.GetValues<AcessoriosVeiculo>()
            .Where(a => a != AcessoriosVeiculo.Nenhum)
            .Select(a => (Rotulo: a.ToString(), Valor: (decimal)lista.Count(v => v.HasFlag(a))))
            .Where(x => x.Valor > 0);
    }

    private static string RotuloRole(string role) => role switch
    {
        "Vendedor" => "Vendedor",
        "Mecanico" => "Mecânico",
        "Recepcionista" => "Recepcionista",
        "ChefeOficina" => "Chefe de oficina",
        "GerenteVendas" => "Gerente de vendas",
        "Admin" => "Administrador",
        _ => role
    };

    private static string NomeCliente(Dictionary<Guid, string> clientesPorId, Guid clienteId)
        => clientesPorId.TryGetValue(clienteId, out var nome) ? nome : "Cliente removido";

    /// <summary>
    /// Reduz uma distribuição às N maiores categorias — sem agregar o resto
    /// em "Outros" (o padrão do sistema é ocultar silenciosamente o que não
    /// entra no top, nunca somar num pseudo-categoria; ver
    /// docs/redesign/13-padrao-graficos.md). Quem decide entre pizza
    /// (categorias <= 6, mostra todas) e barra (top 10) é o JS
    /// (carstoreChart.comparativo) — este corte aqui é só um teto de
    /// segurança pro tamanho do payload.
    /// </summary>
    private static List<CategoriaValorDTO> Top(
        IEnumerable<(string Rotulo, decimal Valor)> itens, int top = TOP_CATEGORIAS)
        => itens.Where(i => i.Valor > 0)
            .OrderByDescending(i => i.Valor)
            .Take(top)
            .Select(i => new CategoriaValorDTO { Rotulo = i.Rotulo, Valor = i.Valor })
            .ToList();

    /// <summary>
    /// Agrupa lançamentos por mês, preenchendo meses sem movimento com zero.
    /// Resultado tem exatamente <paramref name="meses"/> entradas, em ordem cronológica.
    /// </summary>
    private static List<MesValorDTO> AgruparPorMes(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime janelaInicio,
        int meses = JANELA_MESES_PADRAO)
    {
        var ci = CultureInfo.GetCultureInfo("pt-BR");
        var serie = new List<MesValorDTO>();

        var agrupado = lancamentos
            .GroupBy(x => new { x.Data.Year, x.Data.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(x => x.Valor));

        for (int i = 0; i < meses; i++)
        {
            var mes = janelaInicio.AddMonths(i);
            var chave = (mes.Year, mes.Month);
            var valor = agrupado.TryGetValue(chave, out var v) ? v : 0m;
            serie.Add(new MesValorDTO
            {
                Label = $"{ci.DateTimeFormat.AbbreviatedMonthNames[mes.Month - 1]}/{mes.Year % 100:D2}",
                Valor = valor
            });
        }
        return serie;
    }
}
