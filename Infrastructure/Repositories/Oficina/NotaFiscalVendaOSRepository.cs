using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class NotaFiscalVendaOSRepository : INotaFiscalVendaOSRepository
{
    private readonly AppDbContext _context;
    public NotaFiscalVendaOSRepository(AppDbContext context) => _context = context;

    public async Task<NotaFiscalVendaOS?> GetByIdAsync(Guid id)
        => await _context.NotasFiscaisVendaOS.FindAsync(id);

    public async Task<IEnumerable<NotaFiscalVendaOS>> GetAllAsync()
        => await _context.NotasFiscaisVendaOS
            .OrderByDescending(n => n.DataEmissao)
            .ToListAsync();

    public async Task<NotaFiscalVendaOS?> ObterPorOrdemAsync(Guid ordemId)
        => await _context.NotasFiscaisVendaOS
            .FirstOrDefaultAsync(n => n.OrdemServicoId == ordemId);

    public async Task<int> ContarPorAnoAsync(int ano)
        => await _context.NotasFiscaisVendaOS
            .CountAsync(n => n.DataEmissao.Year == ano);

    public async Task AddAsync(NotaFiscalVendaOS entity)
        => await _context.NotasFiscaisVendaOS.AddAsync(entity);

    public void Update(NotaFiscalVendaOS entity) => _context.NotasFiscaisVendaOS.Update(entity);
    public void Remove(NotaFiscalVendaOS entity) => _context.NotasFiscaisVendaOS.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
