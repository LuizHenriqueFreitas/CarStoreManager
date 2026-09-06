using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema.TemplateDocumento;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface ITemplateDocumentoService
{
    Task<Result<IEnumerable<TemplateDocumentoDTO>>> GetAllAsync();
    Task<Result<IEnumerable<TemplateDocumentoLookupDTO>>> GetLookupAtivosAsync();
    Task<Result<TemplateDocumentoDTO>> GetByIdAsync(Guid id);
    Task<Result<Guid>> AddAsync(SalvarTemplateDocumentoDTO dto);
    Task<Result> UpdateAsync(SalvarTemplateDocumentoDTO dto);
    Task<Result> RemoveAsync(Guid id);
}
