namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class PropostaVendaDTO
{
    public Guid Id { get; set; }
    public Guid VendedorId { get; set; }
    public Guid VeiculoVendaId { get; set; }
    public Guid ClienteId { get; set; }
    public decimal ValorBase { get; set; }
    public decimal DescontoPercentual { get; set; }
    public decimal ValorFinal { get; set; }
    public decimal Entrada { get; set; }
    public decimal ValorFinanciado { get; set; }
    public int Parcelas { get; set; }
    public DateTime DataCriacao { get; set; }
    public string Status { get; set; } = null!;
}