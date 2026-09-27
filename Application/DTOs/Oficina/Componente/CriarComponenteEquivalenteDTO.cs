namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class CriarComponenteEquivalenteDTO
{
    public Guid ComponenteOriginalId { get; set; }
    public Guid ComponenteEquivalenteId { get; set; }
    public CarStoreManager.Domain.Enums.TipoEquivalencia TipoEquivalencia { get; set; }
}
