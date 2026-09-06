namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class DefinirModoPagamentoDTO
{
    public string ModoPagamento { get; set; } = "";
}

public class RegistrarRespostaFinanciadoraDTO
{
    /// <summary>Texto livre com o que a financiadora propôs — anotado pelo vendedor após contato feito por fora do sistema.</summary>
    public string TextoProposta { get; set; } = "";
}

public class NegarFinanciamentoDTO
{
    public string Motivo { get; set; } = "";
}

public class RejeitarPropostaDTO
{
    public string Motivo { get; set; } = "";
}

public class CancelarPropostaDTO
{
    public string Motivo { get; set; } = "";
}
