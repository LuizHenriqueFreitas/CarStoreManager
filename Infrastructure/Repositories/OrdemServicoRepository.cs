using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly AppDbContext _context;

    public OrdemServicoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrdemServico>> ObterTodasAsync()
    {
        return await _context.OrdensServico
            .Include(o => o.ClienteId)
            .Include(o => o.VeiculoId)
            .ToListAsync();
    }

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id)
    {
        return await _context.OrdensServico
            .Include(o => o.ClienteId)
            .Include(o => o.VeiculoId)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AdicionarAsync(OrdemServico ordem)
    {
        await _context.OrdensServico.AddAsync(ordem);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(OrdemServico ordem)
    {
        _context.OrdensServico.Update(ordem);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var ordem = await _context.OrdensServico.FindAsync(id);

        if (ordem != null)
        {
            _context.OrdensServico.Remove(ordem);
            await _context.SaveChangesAsync();
        }
    }
}