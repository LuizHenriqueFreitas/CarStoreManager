using CarStoreManager.Domain.Entities.Sistema;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface ITipoDespesaRepository
{
    Task<TipoDespesa?> GetByIdAsync(Guid id);
    Task<IEnumerable<TipoDespesa>> GetAllAsync();
    Task<IEnumerable<TipoDespesa>> GetAtivosAsync();
    Task AddAsync(TipoDespesa tipo);
    void Update(TipoDespesa tipo);
    void Remove(TipoDespesa tipo);
    Task SaveChangesAsync();
}
