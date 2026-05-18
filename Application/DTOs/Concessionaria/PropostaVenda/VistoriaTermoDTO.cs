namespace CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

public class VistoriaDTO
{
    public Guid Id { get; set; }
    public Guid PropostaVendaId { get; set; }
    public Guid VistoriadorId { get; set; }
    public DateTime DataRealizada { get; set; }
    public string Observacoes { get; set; } = "";
    public bool Aprovado { get; set; }
    public bool Concluida { get; set; }
    public DateTime? DataConclusao { get; set; }
}

public class RegistrarVistoriaDTO
{
    public string Observacoes { get; set; } = "";
    public bool Aprovado { get; set; }
}

public class TermoEntregaDTO
{
    public Guid Id { get; set; }
    public Guid PropostaVendaId { get; set; }
    public string TextoTermo { get; set; } = "";
    public string Status { get; set; } = "Rascunho";
    public DateTime DataRedacao { get; set; }
    public DateTime? DataUltimaEdicao { get; set; }
    public string? TokenAssinatura { get; set; }
    public DateTime? DataAssinatura { get; set; }
    public string? AssinaturaNomeCliente { get; set; }
    public string? AssinaturaCpfCliente { get; set; }
    public string? AssinaturaIp { get; set; }

    /// <summary>Valor final do veículo (ValorFinal da proposta).</summary>
    public decimal ValorVeiculo { get; set; }
    /// <summary>Soma dos pagamentos registrados para a proposta.</summary>
    public decimal ValorPago { get; set; }
}

public class CriarOuEditarTermoDTO
{
    public string TextoTermo { get; set; } = "";
}

public class AssinarTermoDTO
{
    public string NomeCliente { get; set; } = "";
    public string CpfCliente { get; set; } = "";
    public bool Aceite { get; set; }
}
