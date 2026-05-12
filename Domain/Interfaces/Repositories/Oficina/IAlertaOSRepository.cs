using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface IAlertaOSRepository : IRepository<AlertaOS>
{
    Task<IEnumerable<AlertaOS>> ObterPorOrdemAsync(Guid ordemId);
    Task<IEnumerable<AlertaOS>> ObterPorStatusAsync(StatusAlertaOS status);
}
