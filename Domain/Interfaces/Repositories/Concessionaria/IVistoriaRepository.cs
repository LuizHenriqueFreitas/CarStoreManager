using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;

public interface IVistoriaRepository : IRepository<Vistoria>
{
    Task<IEnumerable<Vistoria>> ObterPorPropostaAsync(Guid propostaId);
}
