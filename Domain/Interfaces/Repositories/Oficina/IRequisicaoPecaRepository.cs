using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface IRequisicaoPecaRepository : IRepository<RequisicaoPecaOS>
{
    Task<IEnumerable<RequisicaoPecaOS>> ObterPorOrdemAsync(Guid ordemId);
    Task<IEnumerable<RequisicaoPecaOS>> ObterPorStatusAsync(StatusRequisicaoPeca status);
    Task<int> ContarPendentesPorOrdemAsync(Guid ordemId);
}
