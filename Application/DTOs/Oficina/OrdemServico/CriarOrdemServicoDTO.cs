namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class CriarOrdemServicoDTO
{
    public Guid VeiculoId { get; set; }
    public Guid MecanicoId { get; set; }
    public Guid ClienteId { get; set; }    

    public string Tipo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public DateTime PrazoEstimado { get; set; }

    public decimal CustoServico { get; set; }

    public List<AdicionarItemOrdemServicoDTO> Itens { get; set; } = new();
}