using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;

namespace CarStoreManager.Application.Interfaces.Oficina;

public interface IChecklistPresetService
{
    Task<Result<IEnumerable<ChecklistPresetDTO>>> GetAllAsync();
    Task<Result<IEnumerable<ChecklistPresetLookupDTO>>> GetLookupAtivosAsync();
    Task<Result<ChecklistPresetDTO>> GetByIdAsync(Guid id);
    Task<Result<Guid>> AddAsync(SalvarChecklistPresetDTO dto);
    Task<Result> UpdateAsync(SalvarChecklistPresetDTO dto);
    Task<Result> RemoveAsync(Guid id);
}
