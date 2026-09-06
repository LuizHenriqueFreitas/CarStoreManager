namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class AnuncioMercadoLivreDTO
{
    public Guid Id { get; set; }
    public string EntidadeTipo { get; set; } = "";
    public Guid EntidadeId { get; set; }
    public string NomeEntidade { get; set; } = "";
    public string ItemIdML { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal UltimoPrecoSincronizado { get; set; }
    public DateTime? DataUltimaSincronizacao { get; set; }
    public string? UltimoErro { get; set; }
    public string? UltimoErroDetalheTecnico { get; set; }
    public string? CategoriaML { get; set; }
    public string? ListingTypeML { get; set; }
}
