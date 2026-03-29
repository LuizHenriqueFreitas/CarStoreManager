using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class MecanicoRepository : IMecanicoRepository
{
    private readonly AppDbContext _context;

    public MecanicoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Mecanico?> GetByIdAsync(Guid id)
        => await _context.Mecanicos.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<Mecanico>> GetAllAsync()
        => await _context.Mecanicos.ToListAsync();

    public async Task<IEnumerable<Mecanico>> ObterPorEspecialidadeAsync(EspecialidadeMecanico especialidade)
        => await _context.Mecanicos
            .Where(x => x.Especialidade == especialidade)
            .ToListAsync();

    public async Task<IEnumerable<Mecanico>> ObterPorNivelAsync(NivelFuncionario nivel)
        => await _context.Mecanicos
            .Where(x => x.DadosFuncionario.Nivel == nivel)
            .ToListAsync();

    public async Task AddAsync(Mecanico mecanico)
        => await _context.Mecanicos.AddAsync(mecanico);

    public void Update(Mecanico mecanico)
        => _context.Mecanicos.Update(mecanico);

    public void Remove(Mecanico mecanico)
        => _context.Mecanicos.Remove(mecanico);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}