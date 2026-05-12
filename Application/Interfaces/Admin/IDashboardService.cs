using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Admin;

namespace CarStoreManager.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardMetricasDTO>> ObterMetricasAsync();
}
