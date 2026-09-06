using CarStoreManager.Domain.Entities.Sistema;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface ITemplateDocumentoRepository
{
    Task<TemplateDocumento?> GetByIdAsync(Guid id);
    Task<IEnumerable<TemplateDocumento>> GetAllAsync();
    Task<IEnumerable<TemplateDocumento>> GetAtivosAsync();
    Task AddAsync(TemplateDocumento template);
    void Update(TemplateDocumento template);
    void Remove(TemplateDocumento template);
    Task SaveChangesAsync();
}
