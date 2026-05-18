namespace CarStoreManager.Application.DTOs.Sistema;

public class DespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public bool Ativa { get; set; }
    public string Setor { get; set; } = "Geral";
    public DateTime? DataUltimaAtualizacao { get; set; }
}

public class CriarDespesaDTO
{
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public string Setor { get; set; } = "Geral";
}

public class AtualizarDespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public bool Ativa { get; set; } = true;
    public string Setor { get; set; } = "Geral";
}
