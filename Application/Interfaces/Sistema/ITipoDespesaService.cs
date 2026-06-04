using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface ITipoDespesaService
{
    Task<Result<IEnumerable<TipoDespesaDTO>>> GetAllAsync();
    Task<Result<IEnumerable<TipoDespesaDTO>>> GetAtivosAsync();
    Task<Result<Guid>> AddAsync(SalvarTipoDespesaDTO dto);
    Task<Result> UpdateAsync(SalvarTipoDespesaDTO dto);
    Task<Result> RemoveAsync(Guid id);
}
