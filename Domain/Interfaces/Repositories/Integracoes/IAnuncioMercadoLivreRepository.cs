using CarStoreManager.Domain.Entities.Integracoes;

namespace CarStoreManager.Domain.Repositories;

public interface IAnuncioMercadoLivreRepository : IRepository<AnuncioMercadoLivre>
{
    Task<AnuncioMercadoLivre?> ObterPorEntidadeAsync(string entidadeTipo, Guid entidadeId);
    Task<AnuncioMercadoLivre?> ObterPorItemIdMLAsync(string itemIdML);
}
