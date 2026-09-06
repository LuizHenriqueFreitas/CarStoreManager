namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

/// <summary>
/// Usado só por IMercadoLivreApiClient.AtualizarItemAsync (PUT de preço/
/// quantidade num item já publicado) — a criação do payload completo de
/// publicação (título, categoria, atributos, buying_mode...) foi pra
/// IConstrutorPayloadAnuncio/PayloadAnuncioMLDTO, que monta o objeto pronto
/// pra API a partir dos metadados reais da categoria, não de um DTO fixo.
/// </summary>
public class MercadoLivreItemDTO
{
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
}
