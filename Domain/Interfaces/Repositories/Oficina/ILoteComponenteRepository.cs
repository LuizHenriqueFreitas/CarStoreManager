using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface ILoteComponenteRepository : IRepository<LoteComponente>
{
    Task<IEnumerable<LoteComponente>> ObterPorComponenteAsync(Guid componenteId);
    Task<IEnumerable<LoteComponente>> ObterPorNotaAsync(Guid notaFiscalId);
}
