namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda.Pagamento;

public class PagamentoPropostaDTO
{
    public Guid Id { get; set; }
    public Guid PropostaVendaId { get; set; }
    public string ModoPagamento { get; set; } = "";
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; }
    public Guid RecebidoPor { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observacoes { get; set; }
}

public class RegistrarPagamentoPropostaDTO
{
    public string ModoPagamento { get; set; } = "";
    public decimal Valor { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Observacoes { get; set; }
}

public class ResumoPagamentoPropostaDTO
{
    public Guid PropostaVendaId { get; set; }
    public decimal ValorTotalProposta { get; set; }
    public decimal ValorPago { get; set; }
    public decimal ValorRestante { get; set; }
    public string StatusPagamento { get; set; } = "Pendente";
    public List<PagamentoPropostaDTO> Pagamentos { get; set; } = new();
}
