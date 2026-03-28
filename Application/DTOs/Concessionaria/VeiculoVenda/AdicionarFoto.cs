namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class AdicionarFotoDTO
{
    public Guid VeiculoId { get; set; }
    public string Url { get; set; } = null!;
}