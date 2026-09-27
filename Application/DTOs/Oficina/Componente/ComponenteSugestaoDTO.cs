namespace CarStoreManager.Application.DTOs.Oficina.Componente;

/// <summary>
/// Sugestão de substituto pra um componente — união de dois critérios:
/// mesmo CodigoOEM (automático, sempre seguro) e vínculo curado manualmente
/// em ComponenteEquivalente (OrigemOEM/OrigemCurada podem ser ambos true
/// quando as duas trilhas acham o mesmo componente).
/// </summary>
public class ComponenteSugestaoDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string MarcaFabricante { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public string CodigoOEM { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal ValorVenda { get; set; }
    public int QuantidadeEmEstoque { get; set; }
    public bool OrigemOEM { get; set; }
    public bool OrigemCurada { get; set; }
    /// <summary>Só preenchido quando OrigemCurada — classificação do vínculo manual.</summary>
    public CarStoreManager.Domain.Enums.TipoEquivalencia? TipoEquivalencia { get; set; }
}
