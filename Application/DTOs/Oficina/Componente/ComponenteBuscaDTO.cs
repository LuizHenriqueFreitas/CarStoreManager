namespace CarStoreManager.Application.DTOs.Oficina.Componente;

/// <summary>
/// Resultado da busca de componentes pelo autocomplete na criação/edição de OS.
/// Inclui o ValorVenda para que o seletor já preencha o preço unitário sugerido
/// sem precisar de uma segunda chamada.
/// </summary>
public class ComponenteBuscaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string SKUInterno { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public string CodigoOEM { get; set; } = "";
    public string MarcaFabricante { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal ValorVenda { get; set; }
    public int QuantidadeEmEstoque { get; set; }
}
