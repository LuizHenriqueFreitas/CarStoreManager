namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

/// <summary>
/// Payload pronto para POST /items, junto com os metadados que
/// <see cref="CarStoreManager.Application.Services.Integracoes.MercadoLivrePublicacaoService"/>
/// precisa depois de publicar (pra gravar em <c>AnuncioMercadoLivre</c> e reutilizar
/// numa republicação) — devolvido pelos construtores de payload
/// (<see cref="CarStoreManager.Application.Interfaces.IConstrutorPayloadAnuncio"/>).
/// </summary>
public class PayloadAnuncioMLDTO
{
    /// <summary>Objeto anônimo/dicionário já no formato que a API do ML espera — serializado direto via JsonContent.Create.</summary>
    public object Payload { get; set; } = new { };

    public string CategoriaId { get; set; } = "";
    public string ListingTypeId { get; set; } = "";
    public decimal Preco { get; set; }
}
