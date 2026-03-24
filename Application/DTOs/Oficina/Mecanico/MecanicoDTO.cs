namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class MecanicoDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string Especialidade { get; set; } = string.Empty;

    public decimal ValorHora { get; set; }

    public string NivelOcupacao { get; set; } = string.Empty;
}