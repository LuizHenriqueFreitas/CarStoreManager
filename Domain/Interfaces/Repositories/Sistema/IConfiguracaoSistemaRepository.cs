using CarStoreManager.Domain.Entities.Sistema;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

/// <summary>
/// Repositório singleton — sempre devolve o único registro do BD.
/// Se não existir, cria automaticamente um vazio na primeira chamada.
/// </summary>
public interface IConfiguracaoSistemaRepository
{
    Task<ConfiguracaoSistema> ObterAsync();
    Task SaveChangesAsync();
}
