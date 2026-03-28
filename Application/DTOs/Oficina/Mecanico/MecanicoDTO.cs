namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class MecanicoDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Especialidade { get; set; } = null!;
    public string Nivel { get; set; } = null!;
    public DateTime DataContratacao { get; set; }
}