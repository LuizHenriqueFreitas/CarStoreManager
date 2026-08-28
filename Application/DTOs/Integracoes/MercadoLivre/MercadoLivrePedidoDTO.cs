using System.Text.Json.Serialization;

namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class MercadoLivrePedidoDTO
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("order_items")]
    public List<MercadoLivreItemPedidoDTO> OrderItems { get; set; } = new();
}

public class MercadoLivreItemPedidoDTO
{
    [JsonPropertyName("item")]
    public MercadoLivreItemRefDTO Item { get; set; } = new();

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }
}

public class MercadoLivreItemRefDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
}
