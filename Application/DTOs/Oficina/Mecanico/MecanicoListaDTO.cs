namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class MecanicoListaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Especialidade { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}