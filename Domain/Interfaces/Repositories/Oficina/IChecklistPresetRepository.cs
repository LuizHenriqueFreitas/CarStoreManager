using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface IChecklistPresetRepository
{
    Task<ChecklistPreset?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChecklistPreset>> GetAllAsync();
    Task<IEnumerable<ChecklistPreset>> GetAtivosAsync();
    Task AddAsync(ChecklistPreset preset);
    void Update(ChecklistPreset preset);
    void Remove(ChecklistPreset preset);
    Task SaveChangesAsync();
}
