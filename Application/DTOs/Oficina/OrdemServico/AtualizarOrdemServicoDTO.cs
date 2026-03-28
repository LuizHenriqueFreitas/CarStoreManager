namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AtualizarStatusChecklistDTO
{
    public Guid OrdemServicoId { get; set; }
    public Guid ItemId { get; set; }
    public string NovoStatus { get; set; } = null!;
}