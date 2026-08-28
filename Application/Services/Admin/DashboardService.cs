using System.Globalization;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Dashboards;

/// <summary>
/// Calcula métricas agregadas para o dashboard administrativo.
/// Faz tudo em memória — adequado para volume de uma loja pequena/média.
/// Para escala maior, mover para queries SQL agregadas.
/// </summary>
public class DashboardService : IDashboardService
{
    private const int JANELA_MESES = 6;
    private const int JANELA_MESES_ACUMULADA = 12;
    private const int TOP_CATEGORIAS = 6;

    private readonly IDespesaRepository _despesas;
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

    public DashboardService(
        IDespesaRepository despesas,
        IOrdemServicoRepository ordens,
        IPropostaVendaRepository propostas,
        IVeiculoVendaRepository veiculos,
        IMecanicoRepository mecanicos,
        IUsuarioRepository usuarios,
        IVeiculoConsignacaoRepository consignacoes,
        IVeiculoClienteRepository veiculosCliente,
        IComponenteRepository componentes,
        IEstoqueRepository estoque,
        IVendaMercadoLivreRepository vendasMercadoLivre)
    {
        _despesas = despesas;
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
    }

    public async Task<Result<DashboardMetricasDTO>> ObterMetricasAsync()
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var janelaInicio = inicioMes.AddMonths(-(JANELA_MESES - 1));

        var dto = new DashboardMetricasDTO();

        // === Despesas fixas mensais cadastradas pelo admin (luz, água, aluguel, salários, etc.) ===
        var despesasAtivas = (await _despesas.GetAtivasAsync()).ToList();
        dto.TotalDespesasFixasMensal = despesasAtivas.Sum(d => d.GetValor());
        dto.TotalDespesasGeralMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Geral)
            .Sum(d => d.GetValor());
        dto.TotalDespesasOficinaMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Oficina)
            .Sum(d => d.GetValor());
        dto.TotalDespesasConcessionariaMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Concessionaria)
            .Sum(d => d.GetValor());

        // === Receita de serviços (OS finalizadas) ===
        var todasOrdens = (await _ordens.GetAllAsync()).ToList();

        dto.OrdensServicoPorStatus = todasOrdens
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var ordensFinalizadas = todasOrdens
            .Where(o => o.Status == StatusOrdemServico.Finalizada)
            .ToList();

        dto.ReceitaServicosMesAtual = ordensFinalizadas
            .Where(o => o.DataCriacao >= inicioMes)
            .Sum(o => o.GetValorTotal());

        dto.SerieReceitaServicos = AgruparPorMes(
            ordensFinalizadas
                .Where(o => o.DataCriacao >= janelaInicio)
                .Select(o => (Data: o.DataCriacao, Valor: o.GetValorTotal())),
            janelaInicio);

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
            .Select(s => new MesValorDTO { MesLabel = s.MesLabel, Valor = acumulado += s.Valor })
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
            janelaInicio);

        // === Propostas aprovadas vs rejeitadas (últimos 6 meses) ===
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
        for (int i = 0; i < JANELA_MESES; i++)
        {
            var mes = janelaInicio.AddMonths(i);
            var chave = (mes.Year, mes.Month);
            timeline.Add(new PropostaTimelineDTO
            {
                MesLabel = $"{ciProp.DateTimeFormat.AbbreviatedMonthNames[mes.Month - 1]}/{mes.Year % 100:D2}",
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

        dto.Graficos = MontarGraficos(
            despesasAtivas, todasOrdens, todasPropostas, propostasFechadas, veiculos,
            todosUsuarios, todasConsignacoes, todosVeiculosCliente, veiculosClientePorId,
            componentesPorId, todoEstoque, todasVendasML, mecanicosLista);

        // Composição financeira do mês — inserida na frente da lista para ser
        // a opção padrão do seletor ("dados financeiros básicos por padrão").
        dto.Graficos.Insert(0, new GraficoAnaliseDTO
        {
            Id = "financeiro-composicao",
            Titulo = "Composição financeira do mês",
            Categoria = "Financeiro",
            TipoGrafico = "doughnut",
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

    public async Task<Result<DashboardMetricasDTO>> ObterMetricasPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        if (dataFim.Date < dataInicio.Date)
            return Result<DashboardMetricasDTO>.Fail("Data final não pode ser anterior à data inicial.");

        var inicioPeriodo = dataInicio.Date;
        var fimPeriodo = dataFim.Date.AddDays(1).AddTicks(-1); // fim do dia, inclusive
        var mesesNoPeriodo = (fimPeriodo.Year - inicioPeriodo.Year) * 12 + fimPeriodo.Month - inicioPeriodo.Month + 1;

        var dto = new DashboardMetricasDTO();

        // === Despesas — são um valor mensal recorrente cadastrado (sem data
        // própria por lançamento), então para um período com mais de um mês o
        // valor é uma ESTIMATIVA: valor mensal cadastrado × nº de meses do
        // período (contagem inclusiva de meses parciais nas pontas). ===
        var despesasAtivas = (await _despesas.GetAtivasAsync()).ToList();
        dto.TotalDespesasFixasMensal = despesasAtivas.Sum(d => d.GetValor()) * mesesNoPeriodo;
        dto.TotalDespesasGeralMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Geral)
            .Sum(d => d.GetValor()) * mesesNoPeriodo;
        dto.TotalDespesasOficinaMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Oficina)
            .Sum(d => d.GetValor()) * mesesNoPeriodo;
        dto.TotalDespesasConcessionariaMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Concessionaria)
            .Sum(d => d.GetValor()) * mesesNoPeriodo;

        // === Ordens de serviço criadas no período ===
        var ordensNoPeriodo = (await _ordens.GetAllAsync())
            .Where(o => o.DataCriacao >= inicioPeriodo && o.DataCriacao <= fimPeriodo)
            .ToList();

        dto.OrdensServicoPorStatus = ordensNoPeriodo
            .GroupBy(o => o.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var ordensFinalizadas = ordensNoPeriodo
            .Where(o => o.Status == StatusOrdemServico.Finalizada)
            .ToList();

        dto.ReceitaServicosMesAtual = ordensFinalizadas.Sum(o => o.GetValorTotal());

        dto.SerieReceitaServicos = AgruparPorMesNoPeriodo(
            ordensFinalizadas.Select(o => (Data: o.DataCriacao, Valor: o.GetValorTotal())),
            inicioPeriodo, fimPeriodo);

        decimal acumulado = 0;
        dto.SerieReceitaServicosAcumulada12m = dto.SerieReceitaServicos
            .Select(s => new MesValorDTO { MesLabel = s.MesLabel, Valor = acumulado += s.Valor })
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

        dto.SerieReceitaVendas = AgruparPorMesNoPeriodo(
            propostasFechadasPeriodo.Select(p => (Data: p.DataAprovacao!.Value, Valor: p.GetValorFinal())),
            inicioPeriodo, fimPeriodo);

        // === Propostas aprovadas vs rejeitadas, mês a mês dentro do período ===
        var aprovadasPorMes = todasPropostas
            .Where(p => p.DataAprovacao.HasValue && p.DataAprovacao.Value >= inicioPeriodo && p.DataAprovacao.Value <= fimPeriodo)
            .GroupBy(p => new { p.DataAprovacao!.Value.Year, p.DataAprovacao.Value.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

        var rejeitadasPorMes = todasPropostas
            .Where(p => p.Status == StatusPropostaVenda.Rejeitada && p.DataCriacao >= inicioPeriodo && p.DataCriacao <= fimPeriodo)
            .GroupBy(p => new { p.DataCriacao.Year, p.DataCriacao.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

        var ciProp = CultureInfo.GetCultureInfo("pt-BR");
        var timeline = new List<PropostaTimelineDTO>();
        var cursor = new DateTime(inicioPeriodo.Year, inicioPeriodo.Month, 1);
        var cursorFim = new DateTime(fimPeriodo.Year, fimPeriodo.Month, 1);
        while (cursor <= cursorFim)
        {
            var chave = (cursor.Year, cursor.Month);
            timeline.Add(new PropostaTimelineDTO
            {
                MesLabel = $"{ciProp.DateTimeFormat.AbbreviatedMonthNames[cursor.Month - 1]}/{cursor.Year % 100:D2}",
                Aprovadas = aprovadasPorMes.TryGetValue(chave, out var a) ? a : 0,
                Rejeitadas = rejeitadasPorMes.TryGetValue(chave, out var r) ? r : 0
            });
            cursor = cursor.AddMonths(1);
        }
        dto.PropostasTimeline = timeline;

        // === Veículos — status/capital são um retrato do estoque ATUAL, não
        // fazem sentido recalculados "no passado", então mantêm leitura atual. ===
        var veiculos = (await _veiculos.GetAllAsync()).ToList();
        var veiculosPorId = veiculos.ToDictionary(v => v.Id);
        dto.CapitalEstoqueVeiculos = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.Valor.GetValorDinheiro());
        dto.VeiculosPorStatus = veiculos
            .GroupBy(v => v.Disponibilidade.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // === Vendas por marca — veículos das propostas concluídas no período ===
        dto.VendasPorMarca = propostasFechadasPeriodo
            .Where(p => veiculosPorId.ContainsKey(p.VeiculoVendaId))
            .GroupBy(p => veiculosPorId[p.VeiculoVendaId].Marca)
            .Select(g => new VendaMarcaDTO { Marca = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .ToList();

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
        List<Domain.Entities.Oficina.Mecanico> mecanicosLista)
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
            TipoGrafico = "doughnut",
            Dados = TopComOutros(
                despesasAtivas.GroupBy(d => d.Tipo.ToString())
                    .Select(g => (Rotulo: g.Key, Valor: g.Sum(d => d.GetValor()))))
        });

        var ticketVeiculoLoja = MediaOuZero(propostasFechadas.Select(p => p.GetValorFinal()));
        var ticketOS = MediaOuZero(todasOrdens.Where(o => o.Status == StatusOrdemServico.Finalizada).Select(o => o.GetValorTotal()));
        var ticketConsignacao = MediaOuZero(consignacoesVendidas.Select(c => c.Comissao.ValorVendaEsperado.GetValorDinheiro()));
        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "valores-medios",
            Titulo = "Valores médios (ticket)",
            Categoria = "Financeiro",
            TipoGrafico = "bar",
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
            TipoGrafico = "doughnut",
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
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
                veiculosVendidos.Select(v => v.Marca).Concat(consignacoesVendidas.Select(c => c.Marca))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "modelos-vendidos",
            Titulo = "Modelos mais vendidos",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
                veiculosVendidos.Select(v => v.Modelo).Concat(consignacoesVendidas.Select(c => c.Modelo))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "cores-vendidas",
            Titulo = "Cores mais vendidas",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
                veiculosVendidos.Select(v => v.Cor).Concat(consignacoesVendidas.Select(c => c.Cor))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "combustivel-vendido",
            Titulo = "Combustível mais vendido",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
                veiculosVendidos.Select(v => v.Combustivel.ToString())
                    .Concat(consignacoesVendidas.Select(c => c.Combustivel.ToString()))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "cambio-vendido",
            Titulo = "Câmbio mais vendido",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
                veiculosVendidos.Select(v => v.Cambio.ToString())
                    .Concat(consignacoesVendidas.Select(c => c.Cambio.ToString()))))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "acessorios-veiculos",
            Titulo = "Acessórios mais comuns no estoque",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarAcessorios(veiculos.Select(v => v.Acessorios)))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "vendas-consignacao-loja",
            Titulo = "Vendas: consignação vs. loja própria",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
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
            TipoGrafico = "doughnut",
            Dados = TopComOutros(
                todasPropostas
                    .Where(p => p.ModoPagamento != ModoPagamento.NaoDefinido)
                    .GroupBy(p => p.ModoPagamento.ToString())
                    .Select(g => (Rotulo: g.Key, Valor: (decimal)g.Count())),
                top: 8)
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "veiculos-status",
            Titulo = "Veículos por status",
            Categoria = "Concessionária",
            TipoGrafico = "doughnut",
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
            TipoGrafico = "doughnut",
            Dados = new()
            {
                new() { Rotulo = "Loja física", Valor = propostasFechadas.Count },
                new() { Rotulo = "E-commerce (Mercado Livre)", Valor = todasVendasML.Count }
            }
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
            TipoGrafico = "doughnut",
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
            TipoGrafico = "doughnut",
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
            TipoGrafico = "doughnut",
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
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
                todasOrdens
                    .Where(o => veiculosClientePorId.ContainsKey(o.VeiculoClienteId))
                    .Select(o => veiculosClientePorId[o.VeiculoClienteId].Marca)))
        });

        graficos.Add(new GraficoAnaliseDTO
        {
            Id = "modelos-visitam-oficina",
            Titulo = "Modelos que mais visitam a oficina",
            Categoria = "Oficina",
            TipoGrafico = "doughnut",
            Dados = TopComOutros(ContarPorTexto(
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
            TipoGrafico = "bar",
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
            TipoGrafico = "doughnut",
            Dados = TopComOutros(estoquePorSistema, top: 8)
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

    /// <summary>
    /// Reduz uma distribuição para as N maiores categorias, somando o resto
    /// em "Outros" — evita gráfico de pizza com dezenas de fatias minúsculas.
    /// </summary>
    private static List<CategoriaValorDTO> TopComOutros(
        IEnumerable<(string Rotulo, decimal Valor)> itens, int top = TOP_CATEGORIAS)
    {
        var ordenado = itens.Where(i => i.Valor > 0).OrderByDescending(i => i.Valor).ToList();
        var principais = ordenado.Take(top)
            .Select(i => new CategoriaValorDTO { Rotulo = i.Rotulo, Valor = i.Valor })
            .ToList();

        var resto = ordenado.Skip(top).Sum(i => i.Valor);
        if (resto > 0) principais.Add(new CategoriaValorDTO { Rotulo = "Outros", Valor = resto });

        return principais;
    }

    /// <summary>
    /// Agrupa lançamentos por mês, preenchendo meses sem movimento com zero.
    /// Resultado tem exatamente <paramref name="meses"/> entradas, em ordem cronológica.
    /// </summary>
    private static List<MesValorDTO> AgruparPorMes(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime janelaInicio,
        int meses = JANELA_MESES)
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
                MesLabel = $"{ci.DateTimeFormat.AbbreviatedMonthNames[mes.Month - 1]}/{mes.Year % 100:D2}",
                Valor = valor
            });
        }
        return serie;
    }
}
