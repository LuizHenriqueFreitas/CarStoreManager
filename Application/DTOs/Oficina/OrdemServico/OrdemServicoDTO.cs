namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class OrdemServicoDTO
{
    public Guid Id { get; set; }

    public Guid ClienteId { get; set; }

    public Guid MecanicoId { get; set; }

    public Guid VeiculoId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public DateTime PrazoEstimado { get; set; }

    public decimal CustoServico { get; set; }

    public decimal ValorTotal { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<ItemOrdemServicoDTO> Itens { get; set; } = new();
}