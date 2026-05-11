namespace CarStoreManager.Application.DTOs.Oficina.Componente;

public class EstoqueComponenteDTO
{
    public Guid Id { get; set; }
    public Guid ComponenteId { get; set; }
    public string ComponenteNome { get; set; } = string.Empty;
    public string ComponentePartNumber { get; set; } = string.Empty;
    public string Sistema { get; set; } = string.Empty;
    public int QuantidadeAtual { get; set; }
    public int QuantidadeMinima { get; set; }
    public bool EstoqueBaixo => QuantidadeAtual <= QuantidadeMinima;
    public bool SemEstoque => QuantidadeAtual <= 0;
}
