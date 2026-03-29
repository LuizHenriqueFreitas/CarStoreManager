namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class ChecklistItemDTO
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Origem { get; set; } = null!;
    public int OrdemExibicao { get; set; }
}