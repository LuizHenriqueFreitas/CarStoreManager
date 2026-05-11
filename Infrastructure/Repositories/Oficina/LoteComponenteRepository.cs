using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class LoteComponenteRepository : ILoteComponenteRepository
{
    private readonly AppDbContext _context;

    public LoteComponenteRepository(AppDbContext context) => _context = context;

    public async Task<LoteComponente?> GetByIdAsync(Guid id)
        => await _context.LotesComponente.FindAsync(id);

    public async Task<IEnumerable<LoteComponente>> GetAllAsync()
        => await _context.LotesComponente
            .Include(l => l.Componente)
            .Include(l => l.Fornecedor)
            .ToListAsync();

    public async Task<IEnumerable<LoteComponente>> ObterPorComponenteAsync(Guid componenteId)
        => await _context.LotesComponente
            .Include(l => l.Fornecedor)
            .Where(l => l.ComponenteId == componenteId)
            .ToListAsync();

    public async Task<IEnumerable<LoteComponente>> ObterPorNotaAsync(Guid notaFiscalId)
        => await _context.LotesComponente
            .Include(l => l.Componente)
            .Where(l => l.NotaFiscalId == notaFiscalId)
            .ToListAsync();

    public async Task AddAsync(LoteComponente entity) => await _context.LotesComponente.AddAsync(entity);
    public void Update(LoteComponente entity) => _context.LotesComponente.Update(entity);
    public void Remove(LoteComponente entity) => _context.LotesComponente.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
