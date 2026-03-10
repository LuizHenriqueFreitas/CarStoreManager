using SistemaEmpresa.Domain.Entities.Oficina;

namespace Domain.Interfaces;

public interface IMecanicoRepository : IRepository<Mecanico>
{
    Task<Mecanico?> GetByCpfAsync(string cpf);
}