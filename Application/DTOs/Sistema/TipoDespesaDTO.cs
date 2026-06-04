namespace CarStoreManager.Application.DTOs.Sistema;

public class TipoDespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; }
    public DateTime? DataUltimaAtualizacao { get; set; }
}

public class SalvarTipoDespesaDTO
{
    /// <summary>Vazio/Empty no POST de criação; preenchido no PUT.</summary>
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
}
