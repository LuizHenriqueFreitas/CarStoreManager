using System.Globalization;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
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

    private readonly IDespesaRepository _despesas;
    private readonly INotaFiscalRepository _notasEntrada;
    private readonly IOrdemServicoRepository _ordens;
    private readonly IPropostaVendaRepository _propostas;
    private readonly IVeiculoVendaRepository _veiculos;

    public DashboardService(
        IDespesaRepository despesas,
        INotaFiscalRepository notasEntrada,
        IOrdemServicoRepository ordens,
        IPropostaVendaRepository propostas,
        IVeiculoVendaRepository veiculos)
    {
        _despesas = despesas;
        _notasEntrada = notasEntrada;
        _ordens = ordens;
        _propostas = propostas;
        _veiculos = veiculos;
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

        // === Gastos com peças (NotaFiscal entrada aprovada) ===
        var todasNotas = (await _notasEntrada.GetAllAsync())
            .Where(n => n.Status == StatusNotaFiscal.Aprovada)
            .ToList();

        dto.GastoPecasMesAtual = todasNotas
            .Where(n => (n.DataAprovacao ?? n.DataImportacao) >= inicioMes)
            .Sum(n => n.ValorTotal);

        dto.SerieGastoPecas = AgruparPorMes(
            todasNotas
                .Where(n => (n.DataAprovacao ?? n.DataImportacao) >= janelaInicio)
                .Select(n => (Data: (n.DataAprovacao ?? n.DataImportacao), n.ValorTotal)),
            janelaInicio);

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

        // === Capital imobilizado em veículos disponíveis ===
        var veiculos = await _veiculos.GetAllAsync();
        dto.CapitalEstoqueVeiculos = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.Valor.GetValorDinheiro());

        return Result<DashboardMetricasDTO>.Ok(dto);
    }

    public async Task<Result<FluxoCaixaPeriodoDTO>> ObterFluxoCaixaPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        // Normaliza para o dia inteiro: [00:00 do início, 23:59:59 do fim].
        // O usuário escolhe datas (não horas) e espera incluir o dia inteiro do fim.
        inicio = inicio.Date;
        fim = fim.Date.AddDays(1).AddTicks(-1);

        if (fim < inicio)
            return Result<FluxoCaixaPeriodoDTO>.Fail("Data fim não pode ser anterior à data início.");

        // Limite generoso pra evitar o admin pedindo 50 anos sem querer.
        if ((fim - inicio).TotalDays > 366 * 10)
            return Result<FluxoCaixaPeriodoDTO>.Fail("Período máximo: 10 anos.");

        // Quantos meses-calendário o intervalo toca (inclusive). Ex: 2026-01-15 → 2026-03-05 = 3 meses.
        var qtdMeses = ((fim.Year - inicio.Year) * 12) + (fim.Month - inicio.Month) + 1;
        var dto = new FluxoCaixaPeriodoDTO
        {
            PeriodoInicio = inicio,
            PeriodoFim = fim,
            QuantidadeMeses = Math.Max(1, qtdMeses)
        };

        // === Despesas fixas (snapshot do valor mensal atual cadastrado) ===
        var despesasAtivas = (await _despesas.GetAtivasAsync()).ToList();
        dto.DespesasFixasGeralMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Geral).Sum(d => d.GetValor());
        dto.DespesasFixasOficinaMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Oficina).Sum(d => d.GetValor());
        dto.DespesasFixasConcessionariaMensal = despesasAtivas
            .Where(d => d.Setor == SetorDespesa.Concessionaria).Sum(d => d.GetValor());

        // === Gasto com peças (notas aprovadas dentro do período) ===
        var notasNoPeriodo = (await _notasEntrada.GetAllAsync())
            .Where(n => n.Status == StatusNotaFiscal.Aprovada)
            .Select(n => (Data: n.DataAprovacao ?? n.DataImportacao, n.ValorTotal))
            .Where(x => x.Data >= inicio && x.Data <= fim)
            .ToList();
        dto.GastoPecas = notasNoPeriodo.Sum(x => x.ValorTotal);

        // === Receita de serviços (OS finalizadas no período) ===
        var ordensFinalizadasNoPeriodo = (await _ordens.GetAllAsync())
            .Where(o => o.Status == StatusOrdemServico.Finalizada
                     && o.DataCriacao >= inicio && o.DataCriacao <= fim)
            .Select(o => (Data: o.DataCriacao, Valor: o.GetValorTotal()))
            .ToList();
        dto.ReceitaServicos = ordensFinalizadasNoPeriodo.Sum(x => x.Valor);

        // === Receita de vendas (Propostas concluídas no período) ===
        var propostasFechadasNoPeriodo = (await _propostas.GetAllAsync())
            .Where(p => p.Status == StatusPropostaVenda.Concluida && p.DataAprovacao.HasValue
                     && p.DataAprovacao.Value >= inicio && p.DataAprovacao.Value <= fim)
            .Select(p => (Data: p.DataAprovacao!.Value, Valor: p.GetValorFinal()))
            .ToList();
        dto.ReceitaVendas = propostasFechadasNoPeriodo.Sum(x => x.Valor);

        // === Séries mês a mês (só os meses tocados pelo período) ===
        dto.SerieReceitaServicos = AgruparPorMesPeriodo(ordensFinalizadasNoPeriodo, inicio, dto.QuantidadeMeses);
        dto.SerieReceitaVendas = AgruparPorMesPeriodo(propostasFechadasNoPeriodo, inicio, dto.QuantidadeMeses);
        dto.SerieGastoPecas = AgruparPorMesPeriodo(notasNoPeriodo, inicio, dto.QuantidadeMeses);

        // === Capital imobilizado (snapshot atual — não dá pra reconstituir histórico) ===
        var veiculos = await _veiculos.GetAllAsync();
        dto.CapitalEstoqueVeiculos = veiculos
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .Sum(v => v.Valor.GetValorDinheiro());

        return Result<FluxoCaixaPeriodoDTO>.Ok(dto);
    }

    /// <summary>
    /// Igual ao AgruparPorMes, mas com janela arbitrária (não JANELA_MESES fixo).
    /// Começa do mês de <paramref name="inicio"/> e gera <paramref name="quantidadeMeses"/> entradas.
    /// </summary>
    private static List<MesValorDTO> AgruparPorMesPeriodo(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime inicio,
        int quantidadeMeses)
    {
        var ci = CultureInfo.GetCultureInfo("pt-BR");
        var primeiroDiaMesInicio = new DateTime(inicio.Year, inicio.Month, 1);
        var serie = new List<MesValorDTO>();

        var agrupado = lancamentos
            .GroupBy(x => new { x.Data.Year, x.Data.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.Valor));

        for (int i = 0; i < quantidadeMeses; i++)
        {
            var mes = primeiroDiaMesInicio.AddMonths(i);
            var valor = agrupado.TryGetValue((mes.Year, mes.Month), out var v) ? v : 0m;
            serie.Add(new MesValorDTO
            {
                MesLabel = $"{ci.DateTimeFormat.AbbreviatedMonthNames[mes.Month - 1]}/{mes.Year % 100:D2}",
                Valor = valor
            });
        }
        return serie;
    }

    /// <summary>
    /// Agrupa lançamentos por mês, preenchendo meses sem movimento com zero.
    /// Resultado tem exatamente JANELA_MESES entradas, em ordem cronológica.
    /// </summary>
    private static List<MesValorDTO> AgruparPorMes(
        IEnumerable<(DateTime Data, decimal Valor)> lancamentos,
        DateTime janelaInicio)
    {
        var ci = CultureInfo.GetCultureInfo("pt-BR");
        var serie = new List<MesValorDTO>();

        var agrupado = lancamentos
            .GroupBy(x => new { x.Data.Year, x.Data.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(x => x.Valor));

        for (int i = 0; i < JANELA_MESES; i++)
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
