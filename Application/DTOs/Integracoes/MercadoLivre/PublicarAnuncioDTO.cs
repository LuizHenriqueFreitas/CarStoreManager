namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class PublicarAnuncioDTO
{
    /// <summary>"VeiculoVenda" | "Componente" | "VeiculoConsignacao"</summary>
    public string EntidadeTipo { get; set; } = "";
    public Guid EntidadeId { get; set; }
}
