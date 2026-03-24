namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AtualizarOrdemServicoDTO
{
    public Guid Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public DateTime PrazoEstimado { get; set; }

    public decimal CustoServico { get; set; }

    public string Status { get; set; } = string.Empty;
}