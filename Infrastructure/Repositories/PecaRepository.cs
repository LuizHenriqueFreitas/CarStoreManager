using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace CarStoreManager.Infrastructure.Repositories;

public class PecaRepository : IComponenteRepository
{
    private readonly AppDbContext _context;

    public PecaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Componente>> ObterTodosAsync()
    {
        return await _context.Pecas.ToListAsync();
    }

    public async Task<Componente?> ObterPorIdAsync(Guid id)
    {
        return await _context.Pecas.FindAsync(id);
    }

    public async Task AdicionarAsync(Componente peca)
    {
        await _context.Pecas.AddAsync(peca);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Componente peca)
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