using CarStoreManager.Domain.Entities.Sistema;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface IPermissaoIndividualRepository
{
    Task<IEnumerable<PermissaoIndividual>> GetAllPorUsuarioAsync(Guid usuarioId);
    Task<PermissaoIndividual?> GetAsync(Guid usuarioId, string recursoChave);
    Task AddAsync(PermissaoIndividual permissao);
    void Update(PermissaoIndividual permissao);
    void Remove(PermissaoIndividual permissao);
    Task SaveChangesAsync();
}
