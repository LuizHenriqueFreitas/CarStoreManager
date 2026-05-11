using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface INotaFiscalRepository : IRepository<NotaFiscal>
{
    Task<NotaFiscal?> ObterPorChaveAsync(string chaveAcesso);
    Task<IEnumerable<NotaFiscal>> ObterPorStatusAsync(StatusNotaFiscal status);
    Task<NotaFiscal?> ObterCompletoAsync(Guid id);
    Task<ItemNotaFiscal?> ObterItemAsync(Guid itemId);
}
