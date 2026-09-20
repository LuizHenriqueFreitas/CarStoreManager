namespace CarStoreManager.Web.Components.Shared;

/// <summary>
/// Rótulos de data em português usados por cards/seções que mostram dado
/// implicitamente filtrado por mês corrente, pra deixar claro pro usuário
/// QUAL mês está sendo exibido (ver docs/redesign/17-margem-financeira-realista.md).
/// </summary>
public static class FormatoData
{
    private static readonly string[] Meses =
    [
        "", "janeiro", "fevereiro", "março", "abril", "maio", "junho",
        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"
    ];

    public static string NomeMes(int mes) => Meses[mes];

    public static string MesAno(DateTime data) => $"{NomeMes(data.Month)}/{data.Year}";
}
