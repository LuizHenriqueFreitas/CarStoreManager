namespace CarStoreManager.Application.DTOs.Sistema.Documentos;

/// <summary>
/// Linha resumida de um documento (termo de entrega, termo de test drive,
/// contrato de consignação ou contrato de OS) para a Central de Documentos —
/// busca por placa/cliente/responsável e exportação em PDF, incluindo fotos
/// quando o documento tem alguma entidade de foto associada.
/// </summary>
public class DocumentoDTO
{
    public Guid Id { get; set; }
    public string TipoDocumento { get; set; } = "";
    public string? ClienteNome { get; set; }
    public string? ResponsavelNome { get; set; }
    public string ResponsavelPapel { get; set; } = "";
    public string? Placa { get; set; }
    public string Status { get; set; } = "";
    public DateTime Data { get; set; }
    public string TextoDocumento { get; set; } = "";

    // Preenchidos só quando o documento tem fotos anexadas — null = sem fotos.
    public string? EntidadeTipoFotos { get; set; }
    public Guid? EntidadeIdFotos { get; set; }
}
