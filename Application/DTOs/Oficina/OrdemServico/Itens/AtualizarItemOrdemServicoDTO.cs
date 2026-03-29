namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AtualizarItemOrdemServicoDTO
{
    public Guid OrdemServicoId { get; set; }
    public Guid ItemId { get; set; }
    public int NovaQuantidade { get; set; }
}
