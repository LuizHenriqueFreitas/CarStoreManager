using CarStoreManager.Domain.Entities.Integracoes;

namespace CarStoreManager.Domain.Interfaces.Repositories.Integracoes;

/// <summary>
/// Repositório singleton — sempre devolve o único registro do BD.
/// Se não existir, cria automaticamente um vazio na primeira chamada.
/// Mesmo padrão de IConfiguracaoSistemaRepository.
/// </summary>
public interface IConfiguracaoMercadoLivreRepository
{
    Task<ConfiguracaoMercadoLivre> ObterAsync();
    Task SaveChangesAsync();
}
