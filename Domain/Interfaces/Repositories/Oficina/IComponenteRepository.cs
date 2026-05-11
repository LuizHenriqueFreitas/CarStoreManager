using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IComponenteRepository : IRepository<Componente>
{
    Task<IEnumerable<Componente>> ObterComEstoqueBaixoAsync();
    Task<IEnumerable<Componente>> ObterPorSistemaAsync(SistemaComponente sistema);
}