using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly AppDbContext _context;

    public EstoqueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EstoqueComponente?> GetByIdAsync(Guid id)
        => await _context.EstoqueComponentes.FindAsync(id);

    public async Task<IEnumerable<EstoqueComponente>> GetAllAsync()
        => await _context.EstoqueComponentes
            .Include(e => e.Componente)
            .ToListAsync();

    public async Task<EstoqueComponente?> ObterPorComponenteAsync(Guid componenteId)
        => await _context.EstoqueComponentes
            .Include(e => e.Componente)
            .FirstOrDefaultAsync(e => e.PecaId == componenteId);

    public async Task<IEnumerable<EstoqueComponente>> ObterComEstoqueBaixoAsync()
        => await _context.EstoqueComponentes
            .Include(e => e.Componente)
            .Where(e => e.QuantidadeAtual <= e.QuantidadeMinima)
            .ToListAsync();

    public async Task AddAsync(EstoqueComponente entity)
        => await _context.EstoqueComponentes.AddAsync(entity);

    public void Update(EstoqueComponente entity)
        => _context.EstoqueComponentes.Update(entity);

    public void Remove(EstoqueComponente entity)
        => _context.EstoqueComponentes.Remove(entity);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
