namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AdicionarItemOrdemServicoDTO
{
    public Guid OrdemServicoId { get; set; }

    public Guid ComponenteId { get; set; }

    public int Quantidade { get; set; }
}