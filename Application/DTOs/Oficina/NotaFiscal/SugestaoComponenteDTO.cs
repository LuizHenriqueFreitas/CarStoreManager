namespace CarStoreManager.Application.DTOs.Oficina.NotaFiscal;

/// <summary>
/// Candidato sugerido para vincular a um item de NF-e que ainda não tem
/// componente associado. Score 0-100 indica confiança da sugestão.
/// </summary>
public class SugestaoComponenteDTO
{
    public Guid ComponenteId { get; set; }
    public string Nome { get; set; } = "";
    public string SKUInterno { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public string CodigoOEM { get; set; } = "";
    public string MarcaFabricante { get; set; } = "";

    /// <summary>0-100. >=80 = match alto; 50-79 = sugestão razoável; &lt;50 = palpite.</summary>
    public int Score { get; set; }

    /// <summary>Critério(s) que casaram (para o admin entender o motivo).</summary>
    public string Motivo { get; set; } = "";
}
