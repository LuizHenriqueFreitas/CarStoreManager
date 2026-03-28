namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class OrdemServicoListaDTO
{
    public Guid Id { get; set; }
    public string NumeroPublico { get; set; } = null!;
    public Guid ClienteId { get; set; }
    public Guid VeiculoClienteId { get; set; }
    public string Tipo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime PrazoEstimado { get; set; }
    public decimal ValorTotal { get; set; }
}