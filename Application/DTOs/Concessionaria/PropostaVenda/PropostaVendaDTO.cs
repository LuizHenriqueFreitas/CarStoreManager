namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class PropostaVendaDTO
{
    public Guid Id { get; set; }
    public Guid VendedorId { get; set; }
    public Guid VeiculoVendaId { get; set; }
    public Guid ClienteId { get; set; }
    public decimal ValorBase { get; set; }
    public decimal DescontoPercentual { get; set; }
    public decimal ValorFinal { get; set; }
    public decimal Entrada { get; set; }
    public decimal ValorLiquidoFinanciamento { get; set; }
    public DateTime DataCriacao { get; set; }
    public string Status { get; set; } = null!;
    public string ModoPagamento { get; set; } = "NaoDefinido";

    // === Financiamento ===
    public DateTime? DataSolicitacaoFinanciamento { get; set; }
    public DateTime? DataRespostaFinanciadora { get; set; }

    /// <summary>Dados do acordo da financiadora, em texto livre.</summary>
    public string? DadosFinanciamento { get; set; }

    // === Auditoria ===
    public DateTime? DataAprovacao { get; set; }
    public string? MotivoRejeicao { get; set; }
    public string? MotivoCancelamento { get; set; }

    /// <summary>Quantos dias faltam até a expiração (negativo = já expirou).</summary>
    public int DiasRestantes { get; set; }
}
