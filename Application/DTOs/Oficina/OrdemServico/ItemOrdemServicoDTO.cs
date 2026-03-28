namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class ItemOrdemServicoDTO
{
    public Guid Id { get; set; }
    public Guid ComponenteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}