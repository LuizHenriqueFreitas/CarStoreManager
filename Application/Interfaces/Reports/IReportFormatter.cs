using CarStoreManager.Application.DTOs.Reports;

namespace CarStoreManager.Application.Interfaces;

public interface IReportFormatter
{
    Task<byte[]> FormatAsync(ReportData data);
}
