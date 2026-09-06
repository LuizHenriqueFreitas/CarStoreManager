using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

namespace CarStoreManager.Application.Interfaces;

public interface IMercadoLivrePublicacaoService
{
    Task<Result<string>> PublicarAsync(PublicarAnuncioDTO dto);
    Task<Result> PausarAsync(Guid anuncioId);
    Task<Result> EncerrarAsync(Guid anuncioId);
    Task<Result<List<AnuncioMercadoLivreDTO>>> ListarAsync();

    /// <summary>
    /// Sugere a categoria pra uma entidade sem publicar — usado pela tela de
    /// anúncios pra exibir a sugestão antes do operador confirmar (e poder
    /// substituí-la manualmente).
    /// </summary>
    Task<Result<SugestaoCategoriaDTO>> SugerirCategoriaAsync(string entidadeTipo, Guid entidadeId);
}
