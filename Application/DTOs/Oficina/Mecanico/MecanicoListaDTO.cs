namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class MecanicoListaDTO
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Especialidade { get; set; } = string.Empty;

    public string NivelOcupacao { get; set; } = string.Empty;
}