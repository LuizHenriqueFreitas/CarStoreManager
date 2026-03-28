namespace CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

public class VeiculoVendaListaDTO
{
    public Guid Id { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public string Combustivel { get; set; } = null!;
    public string Disponibilidade { get; set; } = null!;
    public decimal Valor { get; set; }
    public string? FotoPrincipal { get; set; }
}