namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class CriarPropostaVendaDTO
{
    public Guid VendedorId { get; set; }
    public Guid VeiculoVendaId { get; set; }

    /// <summary>"VeiculoVenda" (padrão) ou "VeiculoConsignacao".</summary>
    public string VeiculoEntidadeTipo { get; set; } = "VeiculoVenda";
    public Guid ClienteId { get; set; }
    public decimal ValorBase { get; set; }
    public decimal DescontoPercentual { get; set; }
}