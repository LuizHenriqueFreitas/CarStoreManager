using System.Globalization;
using System.Text;
using CarStoreManager.Application.DTOs.Reports;
using CarStoreManager.Application.Interfaces;

namespace CarStoreManager.Application.Services.Reports;

/// <summary>
/// Formata um ReportData como CSV (separado por vírgula, com aspas RFC4180
/// quando o valor contém vírgula/aspas/quebra de linha). Independente do CSV
/// legado do RelatorioController (que usa ";" como delimitador).
/// </summary>
public class CsvReportFormatter : IReportFormatter
{
    public Task<byte[]> FormatAsync(ReportData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Relatório: {data.Title}");
        sb.AppendLine($"Gerado em: {data.GeneratedAt:dd/MM/yyyy HH:mm}");
        if (!string.IsNullOrWhiteSpace(data.Periodo))
            sb.AppendLine($"Período: {data.Periodo}");
        sb.AppendLine();

        foreach (var section in data.Sections)
        {
            sb.AppendLine(Escape(section.Name));

            if (section.Rows.Count > 0)
            {
                var headers = section.Rows[0].Keys;
                sb.AppendLine(string.Join(",", headers.Select(Escape)));

                foreach (var row in section.Rows)
                    sb.AppendLine(string.Join(",", row.Values.Select(v => Escape(FormatValue(v)))));
            }

            sb.AppendLine();
        }

        // BOM UTF-8 garante acentuação correta no Excel (mesmo padrão do RelatorioController).
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return Task.FromResult(bom.Concat(body).ToArray());
    }

    private static string FormatValue(object? v) => v switch
    {
        null => "",
        decimal d => d.ToString("F2", CultureInfo.InvariantCulture),
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
        _ => v.ToString() ?? ""
    };

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0) return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
