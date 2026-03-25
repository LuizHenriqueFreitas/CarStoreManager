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

    public async Task<OrdemServico?> GetByIdAsync(Guid id)
    {
        return await _context.OrdensServico
            .Include(o => o.Itens)
            .Include(o => o.Checklist)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<OrdemServico>> GetAllAsync()
    {
        return await _context.OrdensServico
            .Include(o => o.Itens)
            .Include(o => o.Checklist)
            .ToListAsync();
    }

    public async Task<OrdemServico?> ObterPorNumeroPublicoAsync(string numeroPublico)
    {
        return await _context.OrdensServico
            .Include(o => o.Checklist)
            .FirstOrDefaultAsync(o => o.NumeroPublico == numeroPublico);
    }

    public async Task AddAsync(OrdemServico ordem)
    {
        await _context.OrdensServico.AddAsync(ordem);
    }

    public void Update(OrdemServico ordem)
    {
        _context.OrdensServico.Update(ordem);
    }

    public void Remove(OrdemServico ordem)
    {
        _context.OrdensServico.Remove(ordem);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}