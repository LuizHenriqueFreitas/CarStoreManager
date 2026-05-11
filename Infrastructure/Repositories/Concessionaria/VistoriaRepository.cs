using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Concessionaria;

public class VistoriaRepository : IVistoriaRepository
{
    private readonly AppDbContext _context;

    public VistoriaRepository(AppDbContext context) => _context = context;

    public async Task<Vistoria?> GetByIdAsync(Guid id) => await _context.Vistorias.FindAsync(id);
    public async Task<IEnumerable<Vistoria>> GetAllAsync() => await _context.Vistorias.ToListAsync();

    public async Task<IEnumerable<Vistoria>> ObterPorPropostaAsync(Guid propostaId)
        => await _context.Vistorias
            .Where(v => v.PropostaVendaId == propostaId)
            .OrderByDescending(v => v.DataRealizada)
            .ToListAsync();

    public async Task AddAsync(Vistoria entity) => await _context.Vistorias.AddAsync(entity);
    public void Update(Vistoria entity) => _context.Vistorias.Update(entity);
    public void Remove(Vistoria entity) => _context.Vistorias.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
