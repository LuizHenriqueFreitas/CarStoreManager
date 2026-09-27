using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Concessionaria;

public class TermoTestDriveRepository : ITermoTestDriveRepository
{
    private readonly AppDbContext _context;

    public TermoTestDriveRepository(AppDbContext context) => _context = context;

    public async Task<TermoTestDrive?> GetByIdAsync(Guid id) => await _context.TermosTestDrive.FindAsync(id);
    public async Task<IEnumerable<TermoTestDrive>> GetAllAsync() => await _context.TermosTestDrive.ToListAsync();

    public async Task<TermoTestDrive?> ObterPorTestDriveAsync(Guid testDriveId)
        => await _context.TermosTestDrive.FirstOrDefaultAsync(t => t.TestDriveId == testDriveId);

    public async Task<TermoTestDrive?> ObterPorTokenAsync(string token)
        => await _context.TermosTestDrive.FirstOrDefaultAsync(t => t.TokenAssinatura == token);

    public async Task AddAsync(TermoTestDrive entity) => await _context.TermosTestDrive.AddAsync(entity);
    public void Update(TermoTestDrive entity) => _context.TermosTestDrive.Update(entity);
    public void Remove(TermoTestDrive entity) => _context.TermosTestDrive.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
