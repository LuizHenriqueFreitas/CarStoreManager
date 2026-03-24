namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class PropostaVendaListaDTO
{
    public Guid Id { get; set; }

    public string ClienteNome { get; set; } = string.Empty;

    public string Veiculo { get; set; } = string.Empty;

    public decimal ValorFinal { get; set; }

    public string Status { get; set; } = string.Empty;
}