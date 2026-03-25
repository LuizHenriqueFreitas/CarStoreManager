namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AdicionarChecklistItemDTO
{
    public Guid OrdemServicoId { get; set; }
    public string Descricao { get; set; } = null!;
}