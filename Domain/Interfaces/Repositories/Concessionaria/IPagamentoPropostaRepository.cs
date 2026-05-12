using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;

public interface IPagamentoPropostaRepository : IRepository<PagamentoProposta>
{
    Task<IEnumerable<PagamentoProposta>> ObterPorPropostaAsync(Guid propostaId);
}
