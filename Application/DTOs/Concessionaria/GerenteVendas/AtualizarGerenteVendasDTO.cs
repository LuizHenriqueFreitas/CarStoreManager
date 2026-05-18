namespace CarStoreManager.Application.DTOs.Concessionaria.GerenteVendas;

public class AtualizarGerenteVendasDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}
