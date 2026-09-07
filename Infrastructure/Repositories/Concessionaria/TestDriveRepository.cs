using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Concessionaria;

public class TestDriveRepository : ITestDriveRepository
{
    private readonly AppDbContext _context;

    public TestDriveRepository(AppDbContext context) => _context = context;

    public async Task<TestDrive?> GetByIdAsync(Guid id)
        => await _context.TestDrives.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<TestDrive>> GetAllAsync()
        => await _context.TestDrives.OrderByDescending(t => t.DataHora).ToListAsync();

    public async Task<IEnumerable<TestDrive>> ObterPorVeiculoAsync(Guid veiculoVendaId)
        => await _context.TestDrives
            .Where(t => t.VeiculoVendaId == veiculoVendaId)
            .OrderByDescending(t => t.DataHora)
            .ToListAsync();

    public async Task<IEnumerable<TestDrive>> ObterPorClienteAsync(Guid clienteId)
        => await _context.TestDrives
            .Where(t => t.ClienteId == clienteId)
            .OrderByDescending(t => t.DataHora)
            .ToListAsync();

    public async Task AddAsync(TestDrive entity) => await _context.TestDrives.AddAsync(entity);
    public void Update(TestDrive entity) => _context.TestDrives.Update(entity);
    public void Remove(TestDrive entity) => _context.TestDrives.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
