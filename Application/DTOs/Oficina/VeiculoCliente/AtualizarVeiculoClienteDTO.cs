namespace CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;

public class AtualizarVeiculoClienteDTO
{
    public Guid Id { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string Cor { get; set; } = null!;
    public int Ano { get; set; }
}