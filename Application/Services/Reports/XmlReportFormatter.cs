using System.Globalization;
using System.Xml.Linq;
using CarStoreManager.Application.DTOs.Reports;
using CarStoreManager.Application.Interfaces;

namespace CarStoreManager.Application.Services.Reports;

/// <summary>
/// Formata um ReportData como XML aninhado: Relatorio/Secao[]/Item[]/{coluna}.
/// </summary>
public class XmlReportFormatter : IReportFormatter
{
    public Task<byte[]> FormatAsync(ReportData data)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Relatorio",
                new XElement("Titulo", data.Title),
                new XElement("GeradoEm", data.GeneratedAt.ToString("yyyy-MM-dd HH:mm")),
                new XElement("Periodo", data.Periodo),
                data.Sections.Select(s =>
                    new XElement("Secao",
                        new XAttribute("nome", s.Name),
                        s.Rows.Select(row =>
                            new XElement("Item",
                                row.Select(kvp => new XElement(kvp.Key, FormatValue(kvp.Value)))
                            )
                        )
                    )
                )
            )
        );

        using var ms = new MemoryStream();
        doc.Save(ms);
        return Task.FromResult(ms.ToArray());
    }

    private static string FormatValue(object? v) => v switch
    {
        null => "",
        decimal d => d.ToString("F2", CultureInfo.InvariantCulture),
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
        _ => v.ToString() ?? ""
    };
}
