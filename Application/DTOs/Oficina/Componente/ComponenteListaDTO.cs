namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class ComponenteListaDTO
{
    public Guid Id { get; set; }
    public string SKUInterno { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string MarcaFabricante { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Sistema { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
