namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AdicionarItemOrdemServicoDTO
{
    public Guid OrdemServicoId { get; set; }
    public Guid ComponenteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public string Origem { get; set; } = "Estoque"; // Estoque, Cliente ou Encomenda
}
