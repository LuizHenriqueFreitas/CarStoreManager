namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class AplicarDescontoDTO
{
    public Guid PropostaId { get; set; }

    public decimal Percentual { get; set; }
}