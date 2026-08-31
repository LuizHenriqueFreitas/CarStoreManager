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

    /// <summary>
    /// "new" ou "used" — obrigatório em toda publicação no ML (item.condition.required
    /// se omitido). Toda a frota da concessionária já teve emplacamento/IPVA, então é
    /// sempre "used" para veículo; componentes de estoque saem como "new".
    /// </summary>
    public string Condicao { get; set; } = "used";

    /// <summary>
    /// Atributos exigidos pela categoria ML (id -> value_name), ex.: BRAND, MODEL,
    /// VEHICLE_YEAR, KILOMETERS para veículos. A lista completa de atributos
    /// obrigatórios por categoria só pode ser confirmada consultando
    /// GET /categories/{id}/attributes na API real do ML.
    /// </summary>
    public Dictionary<string, string> Atributos { get; set; } = new();
}
