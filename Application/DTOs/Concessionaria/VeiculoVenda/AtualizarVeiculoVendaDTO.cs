namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class AtualizarVeiculoVendaDTO
{
    public Guid Id { get; set; }
    public string Disponibilidade { get; set; } = null!;
    public decimal Valor { get; set; }
}