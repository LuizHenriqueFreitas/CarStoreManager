using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class PermissaoAcessoRepository : IPermissaoAcessoRepository
{
    private readonly AppDbContext _context;

    public PermissaoAcessoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<PermissaoAcesso>> GetAllAsync()
        => await _context.PermissoesAcesso.ToListAsync();

    public async Task<PermissaoAcesso?> GetAsync(RoleUsuario role, string recursoChave)
        => await _context.PermissoesAcesso
            .FirstOrDefaultAsync(p => p.Role == role && p.RecursoChave == recursoChave);

    public async Task AddAsync(PermissaoAcesso permissao) => await _context.PermissoesAcesso.AddAsync(permissao);

    public void Update(PermissaoAcesso permissao) => _context.PermissoesAcesso.Update(permissao);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
