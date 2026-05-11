using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class NotaFiscalRepository : INotaFiscalRepository
{
    private readonly AppDbContext _context;

    public NotaFiscalRepository(AppDbContext context) => _context = context;

    public async Task<NotaFiscal?> GetByIdAsync(Guid id)
        => await _context.NotasFiscais
            .Include(n => n.Itens)
            .Include(n => n.Fornecedor)
            .FirstOrDefaultAsync(n => n.Id == id);

    public async Task<NotaFiscal?> ObterCompletoAsync(Guid id)
        => await _context.NotasFiscais
            .Include(n => n.Itens)
                .ThenInclude(i => i.Componente)
            .Include(n => n.Fornecedor)
            .FirstOrDefaultAsync(n => n.Id == id);

    public async Task<IEnumerable<NotaFiscal>> GetAllAsync()
        => await _context.NotasFiscais
            .Include(n => n.Fornecedor)
            .OrderByDescending(n => n.DataImportacao)
            .ToListAsync();

    public async Task<NotaFiscal?> ObterPorChaveAsync(string chaveAcesso)
        => await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.ChaveAcesso == chaveAcesso);

    public async Task<IEnumerable<NotaFiscal>> ObterPorStatusAsync(StatusNotaFiscal status)
        => await _context.NotasFiscais
            .Include(n => n.Fornecedor)
            .Where(n => n.Status == status)
            .OrderByDescending(n => n.DataImportacao)
            .ToListAsync();

    public async Task<ItemNotaFiscal?> ObterItemAsync(Guid itemId)
        => await _context.ItensNotaFiscal
            .Include(i => i.NotaFiscal)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task AddAsync(NotaFiscal entity) => await _context.NotasFiscais.AddAsync(entity);
    public void Update(NotaFiscal entity) => _context.NotasFiscais.Update(entity);
    public void Remove(NotaFiscal entity) => _context.NotasFiscais.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
