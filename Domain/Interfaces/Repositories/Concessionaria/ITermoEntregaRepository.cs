using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;

public interface ITermoEntregaRepository : IRepository<TermoEntrega>
{
    Task<TermoEntrega?> ObterPorPropostaAsync(Guid propostaId);
    Task<TermoEntrega?> ObterPorTokenAsync(string token);
}
