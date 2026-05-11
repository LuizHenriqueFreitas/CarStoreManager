using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface IPagamentoOrdemServicoRepository : IRepository<PagamentoOrdemServico>
{
    Task<IEnumerable<PagamentoOrdemServico>> ObterPorOrdemAsync(Guid ordemId);
}
