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
        => await _context.Usuarios
            .OfType<Mecanico>()
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Mecanico>> GetAllAsync()
        => await _context.Usuarios
            .OfType<Mecanico>()
            .ToListAsync();

    public async Task<IEnumerable<Mecanico>> ObterPorEspecialidadeAsync(
        EspecialidadeMecanico especialidade)
        => await _context.Usuarios
            .OfType<Mecanico>()
            .Where(m => m.Especialidade == especialidade)
            .ToListAsync();

    public async Task<IEnumerable<Mecanico>> ObterPorNivelAsync(NivelFuncionario nivel)
    {
        // EF não traduz método de OwnedType — trazemos para memória e filtramos.
        var todos = await _context.Usuarios.OfType<Mecanico>().ToListAsync();
        return todos.Where(m => m.DadosFuncionario.GetNivel() == nivel);
    }

    public async Task<IEnumerable<Mecanico>> ObterDisponiveisAsync()
        => await _context.Usuarios
            .OfType<Mecanico>()
            .Where(m => m.Ativo &&
                        m.Ocupado == NivelOcupacaoMecanico.Disponivel)
            .ToListAsync();

    public async Task AddAsync(Mecanico mecanico)
        => await _context.Usuarios.AddAsync(mecanico);

    public void Update(Mecanico mecanico)
        => _context.Usuarios.Update(mecanico);

    public void Remove(Mecanico mecanico)
        => _context.Usuarios.Remove(mecanico);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}