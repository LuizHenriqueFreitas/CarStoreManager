namespace CarStoreManager.Application.DTOs.Oficina.Componente;

/// <summary>
/// Linha da lista de curadoria de equivalências de um componente — o "outro
/// lado" de cada vínculo, já enriquecido com estoque e situação (ativo/descontinuado).
/// </summary>
public class ComponenteEquivalenteDTO
{
    /// <summary>Id do vínculo (ComponenteEquivalente.Id) — usado pra remover.</summary>
    public Guid Id { get; set; }
    public Guid ComponenteRelacionadoId { get; set; }
    public string Nome { get; set; } = "";
    public string MarcaFabricante { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public CarStoreManager.Domain.Enums.TipoEquivalencia TipoEquivalencia { get; set; }
    /// <summary>Do componente relacionado — false = descontinuado.</summary>
    public bool Ativo { get; set; }
    public int QuantidadeEmEstoque { get; set; }
}
