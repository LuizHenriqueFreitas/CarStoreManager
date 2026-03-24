namespace CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

public class VendedorDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;
}