using CarStoreManager.Domain.Entities.Sistema;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface IDespesaRepository
{
    Task<Despesa?> GetByIdAsync(Guid id);
    Task<IEnumerable<Despesa>> GetAllAsync();
    Task<IEnumerable<Despesa>> GetAtivasAsync();

    /// <summary>Despesas (ativas e inativas) de um tipo — usado ao remover um tipo.</summary>
    Task<IEnumerable<Despesa>> GetPorTipoAsync(Guid tipoDespesaId);

    Task AddAsync(Despesa despesa);
    void Update(Despesa despesa);
    void Remove(Despesa despesa);
    Task SaveChangesAsync();
}
