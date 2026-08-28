using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Reports;

namespace CarStoreManager.Application.Interfaces;

public interface IReportService
{
    Task<Result<byte[]>> ExportAsync(ReportType type, string format, DateTime dataInicio, DateTime dataFim);
}
