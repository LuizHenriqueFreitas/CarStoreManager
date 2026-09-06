using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class TemplateDocumentoRepository : ITemplateDocumentoRepository
{
    private readonly AppDbContext _context;

    public TemplateDocumentoRepository(AppDbContext context) => _context = context;

    public async Task<TemplateDocumento?> GetByIdAsync(Guid id)
        => await _context.TemplatesDocumento.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<TemplateDocumento>> GetAllAsync()
        => await _context.TemplatesDocumento.OrderBy(t => t.Nome).ToListAsync();

    public async Task<IEnumerable<TemplateDocumento>> GetAtivosAsync()
        => await _context.TemplatesDocumento.Where(t => t.Ativo).OrderBy(t => t.Nome).ToListAsync();

    public async Task AddAsync(TemplateDocumento template)
        => await _context.TemplatesDocumento.AddAsync(template);

    public void Update(TemplateDocumento template) => _context.TemplatesDocumento.Update(template);

    public void Remove(TemplateDocumento template) => _context.TemplatesDocumento.Remove(template);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
