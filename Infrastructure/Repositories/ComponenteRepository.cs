using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class ComponenteRepository : IComponenteRepository
{
    private readonly AppDbContext _context;

    public ComponenteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Componente?> GetByIdAsync(Guid id)
        => await _context.Componentes.FindAsync(id);

    public async Task<IEnumerable<Componente>> GetAllAsync()
        => await _context.Componentes.ToListAsync();

    public async Task<IEnumerable<Componente>> ObterComEstoqueBaixoAsync()
        => await _context.Componentes
            .Where(c => c.QuantidadeEstoque <= c.EstoqueMinimo)
            .ToListAsync();

    public async Task<IEnumerable<Componente>> ObterPorSistemaAsync(SistemaComponente sistema)
        => await _context.Componentes
            .Where(c => c.Sistema == sistema)
            .ToListAsync();

    public async Task AddAsync(Componente componente)
        => await _context.Componentes.AddAsync(componente);

    public void Update(Componente componente)
        => _context.Componentes.Update(componente);

    public void Remove(Componente componente)
        => _context.Componentes.Remove(componente);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}