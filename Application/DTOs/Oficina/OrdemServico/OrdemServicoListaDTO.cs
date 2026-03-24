namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class OrdemServicoListaDTO
{
    public Guid Id { get; set; }

    public string NomeCliente { get; set; } = string.Empty;

    public string Veiculo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime PrazoEstimado { get; set; }

    public decimal ValorTotal { get; set; }
}