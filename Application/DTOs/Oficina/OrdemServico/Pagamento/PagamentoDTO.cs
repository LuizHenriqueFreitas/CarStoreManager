namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;

public class PagamentoDTO
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public string ModoPagamento { get; set; } = "";
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; }
    public Guid RecebidoPor { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observacoes { get; set; }
}

public class RegistrarPagamentoDTO
{
    public string ModoPagamento { get; set; } = "";
    public decimal Valor { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observacoes { get; set; }
}

public class ResumoPagamentoOrdemDTO
{
    public Guid OrdemServicoId { get; set; }
    public decimal ValorTotalOrdem { get; set; }
    public decimal ValorPago { get; set; }
    public decimal ValorRestante { get; set; }
    public string StatusPagamento { get; set; } = "Pendente";
    public List<PagamentoDTO> Pagamentos { get; set; } = new();
}
