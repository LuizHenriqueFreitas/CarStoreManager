using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Domain.Entities.Integracoes;

namespace CarStoreManager.Application.Mappings.Integracoes;

public static class AnuncioMercadoLivreMapping
{
    public static AnuncioMercadoLivreDTO ToDto(AnuncioMercadoLivre entity, string nomeEntidade)
    {
        return new AnuncioMercadoLivreDTO
        {
            Id = entity.Id,
            EntidadeTipo = entity.EntidadeTipo,
            EntidadeId = entity.EntidadeId,
            NomeEntidade = nomeEntidade,
            ItemIdML = entity.ItemIdML,
            Status = entity.Status.ToString(),
            UltimoPrecoSincronizado = entity.UltimoPrecoSincronizado,
            DataUltimaSincronizacao = entity.DataUltimaSincronizacao,
            UltimoErro = entity.UltimoErro,
            UltimoErroDetalheTecnico = entity.UltimoErroDetalheTecnico,
            CategoriaML = entity.CategoriaML,
            ListingTypeML = entity.ListingTypeML
        };
    }
}
