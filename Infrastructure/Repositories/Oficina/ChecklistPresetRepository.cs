using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class ChecklistPresetRepository : IChecklistPresetRepository
{
    private readonly AppDbContext _context;

    public ChecklistPresetRepository(AppDbContext context) => _context = context;

    public async Task<ChecklistPreset?> GetByIdAsync(Guid id)
        => await _context.ChecklistPresets
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<ChecklistPreset>> GetAllAsync()
        => await _context.ChecklistPresets
            .Include(p => p.Itens)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public async Task<IEnumerable<ChecklistPreset>> GetAtivosAsync()
        => await _context.ChecklistPresets
            .Include(p => p.Itens)
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public async Task AddAsync(ChecklistPreset preset)
        => await _context.ChecklistPresets.AddAsync(preset);

    public void Update(ChecklistPreset preset) => _context.ChecklistPresets.Update(preset);

    public void Remove(ChecklistPreset preset) => _context.ChecklistPresets.Remove(preset);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
