using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IEstoqueRepository : IRepository<EstoqueComponente>
{
    Task<EstoqueComponente?> ObterPorComponenteAsync(Guid componenteId);
    Task<IEnumerable<EstoqueComponente>> ObterComEstoqueBaixoAsync();
}
