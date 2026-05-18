using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Oficina;

public class ChefeOficinaRepository : IChefeOficinaRepository
{
    private readonly AppDbContext _context;

    public ChefeOficinaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChefeOficina?> GetByIdAsync(Guid id)
        => await _context.Usuarios
            .OfType<ChefeOficina>()
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<ChefeOficina>> GetAllAsync()
        => await _context.Usuarios
            .OfType<ChefeOficina>()
            .ToListAsync();

    public async Task AddAsync(ChefeOficina chefe)
        => await _context.Usuarios.AddAsync(chefe);

    public void Update(ChefeOficina chefe)
        => _context.Usuarios.Update(chefe);

    public void Remove(ChefeOficina chefe)
        => _context.Usuarios.Remove(chefe);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
