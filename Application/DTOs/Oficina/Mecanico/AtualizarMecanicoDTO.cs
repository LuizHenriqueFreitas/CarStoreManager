namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class AtualizarMecanicoDTO
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Especialidade { get; set; } = null!;
    public string Nivel { get; set; } = null!;
}