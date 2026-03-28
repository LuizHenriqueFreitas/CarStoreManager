//util para seleção do mecanico na ordem de servico

namespace CarStoreManager.Application.DTOs.Oficina.Mecanico;

public class MecanicoLookupDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
}
