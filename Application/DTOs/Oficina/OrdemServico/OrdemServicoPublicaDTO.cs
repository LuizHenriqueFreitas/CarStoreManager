namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class OrdemServicoPublicaDTO
{
    public string NumeroPublico { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime DataCriacao { get; set; }
    public DateTime PrazoEstimado { get; set; }
    public List<ChecklistItemPublicoDTO> Checklist { get; set; } = new();
}