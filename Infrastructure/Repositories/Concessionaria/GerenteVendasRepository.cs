using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Concessionaria;

public class GerenteVendasRepository : IGerenteVendasRepository
{
    private readonly AppDbContext _context;

    public GerenteVendasRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GerenteVendas?> GetByIdAsync(Guid id)
        => await _context.Usuarios
            .OfType<GerenteVendas>()
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<IEnumerable<GerenteVendas>> GetAllAsync()
        => await _context.Usuarios
            .OfType<GerenteVendas>()
            .ToListAsync();

    public async Task AddAsync(GerenteVendas gerente)
        => await _context.Usuarios.AddAsync(gerente);

    public void Update(GerenteVendas gerente)
        => _context.Usuarios.Update(gerente);

    public void Remove(GerenteVendas gerente)
        => _context.Usuarios.Remove(gerente);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
