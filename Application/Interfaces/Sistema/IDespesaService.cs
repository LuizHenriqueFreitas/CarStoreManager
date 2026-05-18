using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface IDespesaService
{
    Task<Result<IEnumerable<DespesaDTO>>> GetAllAsync();
    Task<Result<DespesaDTO>> GetByIdAsync(Guid id);
    Task<Result<Guid>> AddAsync(CriarDespesaDTO dto);
    Task<Result> UpdateAsync(AtualizarDespesaDTO dto);
    Task<Result> RemoveAsync(Guid id);
    Task<Result<decimal>> ObterTotalMensalAsync();
}
