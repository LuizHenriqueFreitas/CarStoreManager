using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class ComponenteEquivalenteRepository : IComponenteEquivalenteRepository
{
    private readonly AppDbContext _context;

    public ComponenteEquivalenteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ComponenteEquivalente?> GetByIdAsync(Guid id)
        => await _context.ComponentesEquivalentes.FindAsync(id);

    public async Task<IEnumerable<ComponenteEquivalente>> GetAllAsync()
        => await _context.ComponentesEquivalentes.ToListAsync();

    public async Task<IEnumerable<ComponenteEquivalente>> ObterPorComponenteAsync(Guid componenteId)
    {
        // Duas queries em vez de um Union — cada lado carrega um Include
        // diferente (o "outro" componente), e EF Core não traduz bem Union
        // com Includes divergentes entre os dois lados. Dataset por
        // componente é pequeno, concatenar em memória é seguro.
        var comoOriginal = await _context.ComponentesEquivalentes
            .Where(e => e.ComponenteOriginalId == componenteId)
            .Include(e => e.ComponenteEquivalenteRelacionado)
            .ToListAsync();

        var comoRelacionado = await _context.ComponentesEquivalentes
            .Where(e => e.ComponenteEquivalenteId == componenteId)
            .Include(e => e.ComponenteOriginal)
            .ToListAsync();

        return comoOriginal.Concat(comoRelacionado);
    }

    public async Task<ComponenteEquivalente?> ObterLigacaoEntreAsync(Guid componenteAId, Guid componenteBId)
        => await _context.ComponentesEquivalentes.FirstOrDefaultAsync(e =>
            (e.ComponenteOriginalId == componenteAId && e.ComponenteEquivalenteId == componenteBId) ||
            (e.ComponenteOriginalId == componenteBId && e.ComponenteEquivalenteId == componenteAId));

    public async Task AddAsync(ComponenteEquivalente equivalencia)
        => await _context.ComponentesEquivalentes.AddAsync(equivalencia);

    public void Update(ComponenteEquivalente equivalencia)
        => _context.ComponentesEquivalentes.Update(equivalencia);

    public void Remove(ComponenteEquivalente equivalencia)
        => _context.ComponentesEquivalentes.Remove(equivalencia);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
