// Infrastructure/Persistence/Repositories/FotoRepository.cs
using CarStoreManager.Application.Interfaces.Repositories;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Persistence.Repositories;

public class FotoRepository : IFotoRepository
{
    private readonly AppDbContext _context;

    public FotoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Foto?> GetByIdAsync(Guid id) =>
        await _context.Fotos.FindAsync(id);

    public async Task<List<Foto>> GetByEntidadeAsync(string entidadeTipo, Guid entidadeId) =>
        await _context.Fotos
            .Where(f => f.EntidadeTipo == entidadeTipo && f.EntidadeId == entidadeId)
            .OrderBy(f => f.Ordem)
            .ToListAsync();

    public async Task AddAsync(Foto foto) =>
        await _context.Fotos.AddAsync(foto);

    public Task UpdateAsync(Foto foto)
    {
        _context.Fotos.Update(foto);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Foto foto)
    {
        _context.Fotos.Remove(foto);
        return Task.CompletedTask;
    }

    public async Task<int> GetNextOrdemAsync(string entidadeTipo, Guid entidadeId)
    {
        var count = await _context.Fotos
            .CountAsync(f => f.EntidadeTipo == entidadeTipo && f.EntidadeId == entidadeId);
        return count;
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}