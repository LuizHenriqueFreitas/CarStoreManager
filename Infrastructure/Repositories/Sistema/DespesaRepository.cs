using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class DespesaRepository : IDespesaRepository
{
    private readonly AppDbContext _context;

    public DespesaRepository(AppDbContext context) => _context = context;

    public async Task<Despesa?> GetByIdAsync(Guid id)
        => await _context.Despesas.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Despesa>> GetAllAsync()
        => await _context.Despesas
            .OrderBy(d => d.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Despesa>> GetAtivasAsync()
        => await _context.Despesas
            .Where(d => d.Ativa)
            .OrderBy(d => d.Nome)
            .ToListAsync();

    public async Task<IEnumerable<Despesa>> GetAtivasPorSetorAsync(SetorDespesa setor)
        => await _context.Despesas
            .Where(d => d.Ativa && d.Setor == setor)
            .OrderBy(d => d.Nome)
            .ToListAsync();

    public async Task AddAsync(Despesa despesa) => await _context.Despesas.AddAsync(despesa);

    public void Update(Despesa despesa) => _context.Despesas.Update(despesa);

    public void Remove(Despesa despesa) => _context.Despesas.Remove(despesa);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
