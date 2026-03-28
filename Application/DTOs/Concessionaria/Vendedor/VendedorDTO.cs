namespace CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

public class VendedorDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataContratacao { get; set; }
    public int AnosEmpresa { get; set; }
}
