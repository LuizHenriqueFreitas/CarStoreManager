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

    // TODO: portar para EstoqueComponente quando o repositório de estoque existir.
    public Task<IEnumerable<Componente>> ObterComEstoqueBaixoAsync()
        => Task.FromResult<IEnumerable<Componente>>(Array.Empty<Componente>());

    // Componente não tem mais propriedade Sistema; o filtro precisa de outro critério.
    public Task<IEnumerable<Componente>> ObterPorSistemaAsync(SistemaComponente sistema)
        => Task.FromResult<IEnumerable<Componente>>(Array.Empty<Componente>());

    public async Task<IEnumerable<Componente>> BuscarAsync(string termo, int limite = 20)
    {
        if (limite <= 0) limite = 20;
        if (limite > 100) limite = 100;

        if (string.IsNullOrWhiteSpace(termo))
        {
            return await _context.Componentes
                .Where(c => c.Ativo)
                .OrderBy(c => c.Nome)
                .Take(limite)
                .ToListAsync();
        }

        var t = termo.Trim().ToLower();

        // EF.Functions.Like é traduzido para LIKE no SQLite com COLLATE NOCASE
        // nas colunas string padrão. Usamos comparações via ToLower() para
        // garantir case-insensitive em todos os providers.
        return await _context.Componentes
            .Where(c => c.Ativo &&
                       (c.Nome.ToLower().Contains(t) ||
                        c.SKUInterno.ToLower().Contains(t) ||
                        c.PartNumber.ToLower().Contains(t) ||
                        c.CodigoOEM.ToLower().Contains(t) ||
                        c.CodigoBarras.ToLower().Contains(t)))
            .OrderBy(c => c.Nome)
            .Take(limite)
            .ToListAsync();
    }

    public async Task AddAsync(Componente componente)
        => await _context.Componentes.AddAsync(componente);

    public void Update(Componente componente)
        => _context.Componentes.Update(componente);

    public void Remove(Componente componente)
        => _context.Componentes.Remove(componente);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}