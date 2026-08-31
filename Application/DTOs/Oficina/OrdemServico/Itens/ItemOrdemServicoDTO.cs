namespace CarStoreManager.Application.DTOs.Oficina.OrdemServico;

public class ItemOrdemServicoDTO
{
    public Guid Id { get; set; }
    /// <summary>Nulo quando Origem="Cliente" — peça trazida pelo cliente, sem cadastro no catálogo (ver DescricaoLivre).</summary>
    public Guid? ComponenteId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string StatusItem { get; set; } = string.Empty;
    public DateTime? DataRecebimento { get; set; }
    /// <summary>Nome digitado na hora — só preenchido quando Origem="Cliente" (ComponenteId nulo).</summary>
    public string? DescricaoLivre { get; set; }
}
