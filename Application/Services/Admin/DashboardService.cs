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
