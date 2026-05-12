using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface INotaFiscalVendaOSRepository : IRepository<NotaFiscalVendaOS>
{
    Task<NotaFiscalVendaOS?> ObterPorOrdemAsync(Guid ordemId);
    Task<int> ContarPorAnoAsync(int ano);
}
