using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Concessionaria;

public class TermoEntregaRepository : ITermoEntregaRepository
{
    private readonly AppDbContext _context;

    public TermoEntregaRepository(AppDbContext context) => _context = context;

    public async Task<TermoEntrega?> GetByIdAsync(Guid id) => await _context.TermosEntrega.FindAsync(id);
    public async Task<IEnumerable<TermoEntrega>> GetAllAsync() => await _context.TermosEntrega.ToListAsync();

    public async Task<TermoEntrega?> ObterPorPropostaAsync(Guid propostaId)
        => await _context.TermosEntrega.FirstOrDefaultAsync(t => t.PropostaVendaId == propostaId);

    public async Task<TermoEntrega?> ObterPorTokenAsync(string token)
        => await _context.TermosEntrega.FirstOrDefaultAsync(t => t.TokenAssinatura == token);

    public async Task AddAsync(TermoEntrega entity) => await _context.TermosEntrega.AddAsync(entity);
    public void Update(TermoEntrega entity) => _context.TermosEntrega.Update(entity);
    public void Remove(TermoEntrega entity) => _context.TermosEntrega.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
