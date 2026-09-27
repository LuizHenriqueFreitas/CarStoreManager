using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class PermissaoIndividualRepository : IPermissaoIndividualRepository
{
    private readonly AppDbContext _context;

    public PermissaoIndividualRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<PermissaoIndividual>> GetAllPorUsuarioAsync(Guid usuarioId)
        => await _context.PermissoesIndividuais.Where(p => p.UsuarioId == usuarioId).ToListAsync();

    public async Task<PermissaoIndividual?> GetAsync(Guid usuarioId, string recursoChave)
        => await _context.PermissoesIndividuais
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.RecursoChave == recursoChave);

    public async Task AddAsync(PermissaoIndividual permissao) => await _context.PermissoesIndividuais.AddAsync(permissao);

    public void Update(PermissaoIndividual permissao) => _context.PermissoesIndividuais.Update(permissao);

    public void Remove(PermissaoIndividual permissao) => _context.PermissoesIndividuais.Remove(permissao);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
