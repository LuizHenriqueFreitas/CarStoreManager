using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class BalancoMensalDespesaRepository : IBalancoMensalDespesaRepository
{
    private readonly AppDbContext _context;

    public BalancoMensalDespesaRepository(AppDbContext context) => _context = context;

    public async Task<BalancoMensalDespesa?> ObterPorCompetenciaAsync(DateOnly competencia)
    {
        var primeiro = new DateOnly(competencia.Year, competencia.Month, 1);
        return await _context.BalancosMensaisDespesa
            .Include(b => b.Itens)
            .FirstOrDefaultAsync(b => b.Competencia == primeiro);
    }

    public async Task<IEnumerable<BalancoMensalDespesa>> ListarAsync()
        => await _context.BalancosMensaisDespesa
            .Include(b => b.Itens)
            .OrderByDescending(b => b.Competencia)
            .ToListAsync();

    public async Task AddAsync(BalancoMensalDespesa balanco)
        => await _context.BalancosMensaisDespesa.AddAsync(balanco);

    public void Update(BalancoMensalDespesa balanco)
        => _context.BalancosMensaisDespesa.Update(balanco);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
