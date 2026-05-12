namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class ComponenteDTO
{
    public Guid Id { get; set; }

    // Controle interno
    public string SKUInterno { get; set; } = string.Empty;

    // Identificação
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    // Fabricante
    public string MarcaFabricante { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string CodigoOEM { get; set; } = string.Empty;

    // Código de barras
    public string CodigoBarras { get; set; } = string.Empty;

    // Fiscal
    public string NCM { get; set; } = string.Empty;
    public string CEST { get; set; } = string.Empty;

    // Classificação
    public string Categoria { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;

    // Sistema do veículo (Motor, Freios, etc.) — string para tráfego em DTO
    public string Sistema { get; set; } = string.Empty;

    // Peso
    public decimal Peso { get; set; }

    // Garantia
    public int GarantiaDias { get; set; }

    // Status
    public bool Ativo { get; set; }

    // Precificação
    public decimal CustoUnitario { get; set; }
    public decimal? MargemLucroPct { get; set; }
    public decimal ValorVenda { get; set; }
}
