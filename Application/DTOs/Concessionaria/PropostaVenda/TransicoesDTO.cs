namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class DefinirModoPagamentoDTO
{
    public string ModoPagamento { get; set; } = "";
}

public class RegistrarRespostaFinanciadoraDTO
{
    /// <summary>
    /// Texto livre com os dados do acordo retornado pela financiadora, digitado
    /// pelo vendedor (parcelas, valor, taxa, condições — sem formato fixo).
    /// </summary>
    public string DadosFinanciamento { get; set; } = "";
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
