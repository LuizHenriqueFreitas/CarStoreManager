using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class TipoDespesaRepository : ITipoDespesaRepository
{
    private readonly AppDbContext _context;

    public TipoDespesaRepository(AppDbContext context) => _context = context;

    public async Task<TipoDespesa?> GetByIdAsync(Guid id)
        => await _context.TiposDespesa.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<TipoDespesa>> GetAllAsync()
        => await _context.TiposDespesa
            .OrderBy(t => t.Nome)
            .ToListAsync();

    public async Task<IEnumerable<TipoDespesa>> GetAtivosAsync()
        => await _context.TiposDespesa
            .Where(t => t.Ativo)
            .OrderBy(t => t.Nome)
            .ToListAsync();

    public async Task AddAsync(TipoDespesa tipo) => await _context.TiposDespesa.AddAsync(tipo);

    public void Update(TipoDespesa tipo) => _context.TiposDespesa.Update(tipo);

    public void Remove(TipoDespesa tipo) => _context.TiposDespesa.Remove(tipo);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
