namespace CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;

public class VeiculoClienteListaDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Descricao { get; set; } = null!;
    public string Cor { get; set; } = null!;
}