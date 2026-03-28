using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
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

    // =========================
    // BASE
    // =========================

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

    // =========================
    // ESPECÍFICOS
    // =========================

    public async Task<OrdemServico?> ObterPorNumeroPublicoAsync(string numeroPublico)
    {
        return await _context.OrdensServico
            .Include(o => o.Checklist)
            .FirstOrDefaultAsync(o => o.NumeroPublico == numeroPublico);
    }

    public async Task<IEnumerable<OrdemServico>> ObterPorMecanicoAsync(Guid mecanicoId)
    {
        return await _context.OrdensServico
            .Where(o => o.MecanicoId == mecanicoId)
            .Include(o => o.Itens)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId)
    {
        return await _context.OrdensServico
            .Where(o => o.ClienteId == clienteId)
            .Include(o => o.Itens)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status)
    {
        return await _context.OrdensServico
            .Where(o => o.Status == status)
            .Include(o => o.Itens)
            .ToListAsync();
    }
}