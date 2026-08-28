namespace CarStoreManager.Application.DTOs.Reports;

public class ReportData
{
    public string Title { get; set; } = "";
    public DateTime GeneratedAt { get; set; }

    /// <summary>Período coberto pelos dados (ex.: "01/06/2026 a 30/06/2026"). Vazio quando não se aplica.</summary>
    public string Periodo { get; set; } = "";
    public List<ReportSection> Sections { get; set; } = new();
}

public class ReportSection
{
    public string Name { get; set; } = "";
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
}
