namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class CriarPropostaVendaDTO
{
    public Guid VendedorId { get; set; }

    public Guid VeiculoId { get; set; }

    public Guid ClienteId { get; set; }

    public decimal ValorBase { get; set; }
}