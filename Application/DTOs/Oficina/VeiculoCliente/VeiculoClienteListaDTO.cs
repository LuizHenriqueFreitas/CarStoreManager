namespace CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;

public class VeiculoClienteListaDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
}