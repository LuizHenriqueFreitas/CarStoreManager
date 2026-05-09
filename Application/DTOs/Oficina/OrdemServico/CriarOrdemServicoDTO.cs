using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class CriarOrdemServicoDTO
{
    public Guid Id { get; set; }
    public Guid VeiculoClienteId { get; set; }
    public Guid MecanicoId { get; set; }
    public Guid ClienteId { get; set; }
    public string Tipo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public DateTime PrazoEstimado { get; set; }
    public decimal CustoServico { get; set; }
    public List<ItemOrdemServicoDTO> Itens { get; set; } =null!;
}
