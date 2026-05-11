namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class DefinirModoPagamentoDTO
{
    public string ModoPagamento { get; set; } = "";
}

public class RegistrarRespostaFinanciadoraDTO
{
    public int Parcelas { get; set; }
    public decimal ValorParcela { get; set; }
    public decimal TaxaJurosMensal { get; set; }
    public string? Observacoes { get; set; }
}

public class RejeitarPropostaDTO
{
    public string Motivo { get; set; } = "";
}

public class CancelarPropostaDTO
{
    public string Motivo { get; set; } = "";
}
