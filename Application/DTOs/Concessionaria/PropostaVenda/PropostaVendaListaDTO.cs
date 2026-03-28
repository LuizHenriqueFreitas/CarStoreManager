namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class PropostaVendaListaDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoVendaId { get; set; }
    public decimal ValorFinal { get; set; }
    public string Status { get; set; } = null!;
    public DateTime DataCriacao { get; set; }
}
