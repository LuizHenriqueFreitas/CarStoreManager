using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SistemaEmpresa.Domain.Entities.Oficina;

namespace CarStoreManager.Infrastructure.Repositories;

public class PecaRepository : IPecaRepository
{
    private readonly AppDbContext _context;

    public PecaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Peca>> ObterTodosAsync()
    {
        return await _context.Pecas.ToListAsync();
    }

    public async Task<Peca?> ObterPorIdAsync(Guid id)
    {
        return await _context.Pecas.FindAsync(id);
    }

    public async Task AdicionarAsync(Peca peca)
    {
        await _context.Pecas.AddAsync(peca);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Peca peca)
    {
        _context.Pecas.Update(peca);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var peca = await _context.Pecas.FindAsync(id);

        if (peca != null)
        {
            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync();
        }
    }
}