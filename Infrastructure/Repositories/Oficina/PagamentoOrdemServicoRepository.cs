using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class PagamentoOrdemServicoRepository : IPagamentoOrdemServicoRepository
{
    private readonly AppDbContext _context;

    public PagamentoOrdemServicoRepository(AppDbContext context) => _context = context;

    public async Task<PagamentoOrdemServico?> GetByIdAsync(Guid id)
        => await _context.PagamentosOrdemServico.FindAsync(id);

    public async Task<IEnumerable<PagamentoOrdemServico>> GetAllAsync()
        => await _context.PagamentosOrdemServico.ToListAsync();

    public async Task<IEnumerable<PagamentoOrdemServico>> ObterPorOrdemAsync(Guid ordemId)
        => await _context.PagamentosOrdemServico
            .Where(p => p.OrdemServicoId == ordemId)
            .OrderBy(p => p.DataPagamento)
            .ToListAsync();

    public async Task AddAsync(PagamentoOrdemServico entity)
        => await _context.PagamentosOrdemServico.AddAsync(entity);

    public void Update(PagamentoOrdemServico entity) => _context.PagamentosOrdemServico.Update(entity);
    public void Remove(PagamentoOrdemServico entity) => _context.PagamentosOrdemServico.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
