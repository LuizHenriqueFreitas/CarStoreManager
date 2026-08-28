namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class MercadoLivreItemDTO
{
    public string Titulo { get; set; } = "";
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
    public string Descricao { get; set; } = "";
    public List<string> UrlsFotos { get; set; } = new();
    public string CategoriaML { get; set; } = "";
}
