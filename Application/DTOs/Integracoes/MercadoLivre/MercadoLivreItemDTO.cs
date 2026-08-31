namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class MercadoLivreItemDTO
{
    public string Titulo { get; set; } = "";
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
    public string Descricao { get; set; } = "";
    public List<string> UrlsFotos { get; set; } = new();
    public string CategoriaML { get; set; } = "";

    /// <summary>
    /// "buy_it_now" (padrão, produtos comuns) ou "classified" — categorias de
    /// classificados no Mercado Livre (veículos, imóveis, serviços) SÓ aceitam
    /// "classified"; enviar "buy_it_now" nelas causa item.buying_mode.invalid.
    /// </summary>
    public string BuyingMode { get; set; } = "buy_it_now";
}
