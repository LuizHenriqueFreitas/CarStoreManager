namespace CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

public class VendedorListaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}