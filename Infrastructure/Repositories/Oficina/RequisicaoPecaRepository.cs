using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class RequisicaoPecaRepository : IRequisicaoPecaRepository
{
    private readonly AppDbContext _context;
    public RequisicaoPecaRepository(AppDbContext context) => _context = context;

    public async Task<RequisicaoPecaOS?> GetByIdAsync(Guid id)
        => await _context.RequisicoesPeca.FindAsync(id);

    public async Task<IEnumerable<RequisicaoPecaOS>> GetAllAsync()
        => await _context.RequisicoesPeca.OrderByDescending(r => r.DataCriacao).ToListAsync();

    public async Task<IEnumerable<RequisicaoPecaOS>> ObterPorOrdemAsync(Guid ordemId)
        => await _context.RequisicoesPeca
            .Where(r => r.OrdemServicoId == ordemId)
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();

    public async Task<IEnumerable<RequisicaoPecaOS>> ObterPorStatusAsync(StatusRequisicaoPeca status)
        => await _context.RequisicoesPeca
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();

    public async Task<int> ContarPendentesPorOrdemAsync(Guid ordemId)
        => await _context.RequisicoesPeca
            .CountAsync(r => r.OrdemServicoId == ordemId && r.Status == StatusRequisicaoPeca.Pendente);

    public async Task AddAsync(RequisicaoPecaOS entity) => await _context.RequisicoesPeca.AddAsync(entity);
    public void Update(RequisicaoPecaOS entity) => _context.RequisicoesPeca.Update(entity);
    public void Remove(RequisicaoPecaOS entity) => _context.RequisicoesPeca.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
