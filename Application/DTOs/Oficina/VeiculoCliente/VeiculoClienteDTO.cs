namespace CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;

public class VeiculoClienteDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string Cor { get; set; } = null!;
    public int Ano { get; set; }
    public string Descricao { get; set; } = null!;
}