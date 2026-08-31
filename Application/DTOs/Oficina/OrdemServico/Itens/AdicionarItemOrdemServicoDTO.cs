namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class AdicionarItemOrdemServicoDTO
{
    public Guid OrdemServicoId { get; set; }
    /// <summary>Ignorado quando Origem="Cliente" (usa DescricaoLivre em vez de um componente do catálogo).</summary>
    public Guid ComponenteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public string Origem { get; set; } = "Estoque"; // Estoque, Cliente ou Encomenda
    /// <summary>Obrigatório quando Origem="Cliente" — nome da peça trazida pelo cliente, digitado na hora.</summary>
    public string? DescricaoLivre { get; set; }
}
