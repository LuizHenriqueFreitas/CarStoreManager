namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class CriarComponenteDTO
{
    public string SKUInterno { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string MarcaFabricante { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string CodigoOEM { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public string NCM { get; set; } = string.Empty;
    public string CEST { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public string Sistema { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public int GarantiaDias { get; set; }

    /// <summary>Custo unitário inicial — pode vir 0; NFs de entrada atualizam.</summary>
    public decimal CustoUnitario { get; set; }

    /// <summary>Margem em % (ex: 30 = 30%). Se null, usa padrão da config para o Sistema.</summary>
    public decimal? MargemLucroPct { get; set; }
}

public class AjustarMargemDTO
{
    public decimal MargemLucroPct { get; set; }
}
