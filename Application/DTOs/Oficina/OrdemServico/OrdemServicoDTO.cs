using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class OrdemServicoDTO
{
    public Guid Id { get; set; }
    public string NumeroPublico { get; set; } = null!;
    public Guid ClienteId { get; set; }
    public Guid VeiculoClienteId { get; set; }
    public Guid MecanicoId { get; set; }
    public string Tipo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public DateTime DataCriacao { get; set; }
    public DateTime PrazoEstimado { get; set; }
    public decimal CustoServico { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = null!;
    public List<ItemOrdemServicoDTO> Itens { get; set; } = new();
    public List<ChecklistItemDTO> Checklist { get; set; } = new();
}
