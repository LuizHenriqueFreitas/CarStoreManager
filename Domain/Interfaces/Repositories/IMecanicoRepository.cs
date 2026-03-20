using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IMecanicoRepository : IRepository<Mecanico>
{
    Task<Mecanico?> GetByCpfAsync(string cpf);
}