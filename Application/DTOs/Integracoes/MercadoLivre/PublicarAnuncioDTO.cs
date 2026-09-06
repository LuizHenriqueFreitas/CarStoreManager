namespace CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

public class PublicarAnuncioDTO
{
    /// <summary>"VeiculoVenda" | "Componente" | "VeiculoConsignacao"</summary>
    public string EntidadeTipo { get; set; } = "";
    public Guid EntidadeId { get; set; }

    /// <summary>
    /// Categoria escolhida manualmente pelo operador na tela de confirmação de
    /// publicação, substituindo a sugestão automática por domain_discovery.
    /// Null = usa a sugestão automática.
    /// </summary>
    public string? CategoriaMLOverride { get; set; }
}
