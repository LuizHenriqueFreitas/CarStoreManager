using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface IDespesaRepository
{
    Task<Despesa?> GetByIdAsync(Guid id);
    Task<IEnumerable<Despesa>> GetAllAsync();
    Task<IEnumerable<Despesa>> GetAtivasAsync();

    /// <summary>Despesas ativas filtradas por setor.</summary>
    Task<IEnumerable<Despesa>> GetAtivasPorSetorAsync(SetorDespesa setor);

    Task AddAsync(Despesa despesa);
    void Update(Despesa despesa);
    void Remove(Despesa despesa);
    Task SaveChangesAsync();
}
