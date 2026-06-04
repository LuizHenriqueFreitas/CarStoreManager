namespace CarStoreManager.Application.DTOs.Sistema;

public class DespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public bool Ativa { get; set; }
    public Guid TipoDespesaId { get; set; }

    /// <summary>Nome do tipo, resolvido para exibição (somente leitura).</summary>
    public string TipoNome { get; set; } = "";

    public DateTime? DataUltimaAtualizacao { get; set; }
}

public class CriarDespesaDTO
{
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public Guid TipoDespesaId { get; set; }
}

public class AtualizarDespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public decimal Valor { get; set; }
    public bool Ativa { get; set; } = true;
    public Guid TipoDespesaId { get; set; }
}
