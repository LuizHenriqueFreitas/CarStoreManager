using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Concessionaria;

public class PagamentoPropostaRepository : IPagamentoPropostaRepository
{
    private readonly AppDbContext _context;

    public PagamentoPropostaRepository(AppDbContext context) => _context = context;

    public async Task<PagamentoProposta?> GetByIdAsync(Guid id)
        => await _context.PagamentosProposta.FindAsync(id);

    public async Task<IEnumerable<PagamentoProposta>> GetAllAsync()
        => await _context.PagamentosProposta.ToListAsync();

    public async Task<IEnumerable<PagamentoProposta>> ObterPorPropostaAsync(Guid propostaId)
        => await _context.PagamentosProposta
            .Where(p => p.PropostaVendaId == propostaId)
            .OrderBy(p => p.DataPagamento)
            .ToListAsync();

    public async Task AddAsync(PagamentoProposta entity)
        => await _context.PagamentosProposta.AddAsync(entity);

    public void Update(PagamentoProposta entity) => _context.PagamentosProposta.Update(entity);
    public void Remove(PagamentoProposta entity) => _context.PagamentosProposta.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
