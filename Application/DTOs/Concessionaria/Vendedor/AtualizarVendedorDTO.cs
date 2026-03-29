namespace CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

public class AtualizarVendedorDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}