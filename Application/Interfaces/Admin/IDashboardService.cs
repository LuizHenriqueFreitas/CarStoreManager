using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;

namespace CarStoreManager.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardMetricasDTO>> ObterMetricasAsync();

    /// <summary>
    /// Consolida receitas, despesas e lucro num intervalo arbitrário escolhido pelo admin.
    /// Usado pelo relatório de fluxo de caixa (CSV). O intervalo é inclusivo em ambas as pontas.
    /// </summary>
    Task<Result<FluxoCaixaPeriodoDTO>> ObterFluxoCaixaPorPeriodoAsync(DateTime inicio, DateTime fim);
}
