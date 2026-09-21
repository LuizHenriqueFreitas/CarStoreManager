using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface IPermissaoAcessoRepository
{
    Task<IEnumerable<PermissaoAcesso>> GetAllAsync();
    Task<PermissaoAcesso?> GetAsync(RoleUsuario role, string recursoChave);
    Task AddAsync(PermissaoAcesso permissao);
    void Update(PermissaoAcesso permissao);
    Task SaveChangesAsync();
}
