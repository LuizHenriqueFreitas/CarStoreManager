using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class RecepcionistaRepository : IRecepcionistaRepository
{
    private readonly AppDbContext _context;

    public RecepcionistaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Recepcionista?> GetByIdAsync(Guid id)
        => await _context.Usuarios
            .OfType<Recepcionista>()
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Recepcionista>> GetAllAsync()
        => await _context.Usuarios
            .OfType<Recepcionista>()
            .ToListAsync();

    public async Task AddAsync(Recepcionista rec)
        => await _context.Usuarios.AddAsync(rec);

    public void Update(Recepcionista rec)
        => _context.Usuarios.Update(rec);

    public void Remove(Recepcionista rec)
        => _context.Usuarios.Remove(rec);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
