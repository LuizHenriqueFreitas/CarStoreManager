using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;

namespace CarStoreManager.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardMetricasDTO>> ObterMetricasAsync();

    /// <summary>
    /// Recalcula as métricas financeiras/operacionais (sem o catálogo de
    /// gráficos comparativos, que é específico da dashboard interativa) para
    /// um intervalo de datas arbitrário — usado pelos relatórios "completo"
    /// exportáveis, que deixam o usuário escolher o período.
    /// </summary>
    Task<Result<DashboardMetricasDTO>> ObterMetricasPeriodoAsync(DateTime dataInicio, DateTime dataFim);
}
