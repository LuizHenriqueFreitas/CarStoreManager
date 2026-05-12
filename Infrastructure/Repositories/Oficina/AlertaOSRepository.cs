using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class AlertaOSRepository : IAlertaOSRepository
{
    private readonly AppDbContext _context;
    public AlertaOSRepository(AppDbContext context) => _context = context;

    public async Task<AlertaOS?> GetByIdAsync(Guid id) => await _context.AlertasOS.FindAsync(id);

    public async Task<IEnumerable<AlertaOS>> GetAllAsync()
        => await _context.AlertasOS.OrderByDescending(a => a.DataCriacao).ToListAsync();

    public async Task<IEnumerable<AlertaOS>> ObterPorOrdemAsync(Guid ordemId)
        => await _context.AlertasOS
            .Where(a => a.OrdemServicoId == ordemId)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();

    public async Task<IEnumerable<AlertaOS>> ObterPorStatusAsync(StatusAlertaOS status)
        => await _context.AlertasOS
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();

    public async Task AddAsync(AlertaOS entity) => await _context.AlertasOS.AddAsync(entity);
    public void Update(AlertaOS entity) => _context.AlertasOS.Update(entity);
    public void Remove(AlertaOS entity) => _context.AlertasOS.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
