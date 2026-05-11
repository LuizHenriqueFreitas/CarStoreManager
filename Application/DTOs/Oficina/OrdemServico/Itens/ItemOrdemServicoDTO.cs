namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class ItemOrdemServicoDTO
{
    public Guid Id { get; set; }
    public Guid ComponenteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string StatusItem { get; set; } = string.Empty;
    public DateTime? DataRecebimento { get; set; }
}
