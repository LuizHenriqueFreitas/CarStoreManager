using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

namespace CarStoreManager.Application.Interfaces;

public interface IMercadoLivrePublicacaoService
{
    Task<Result<string>> PublicarAsync(PublicarAnuncioDTO dto);
    Task<Result> PausarAsync(Guid anuncioId);
    Task<Result> EncerrarAsync(Guid anuncioId);
    Task<Result<List<AnuncioMercadoLivreDTO>>> ListarAsync();
}
