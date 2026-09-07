namespace CarStoreManager.Application.DTOs.Sistema;

public class ConfigFechamentoDespesaDTO
{
    public int DiaFechamento { get; set; } = 5;
}

public class BalancoMensalDespesaDTO
{
    public Guid Id { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }
    public bool Fechado { get; set; }
    public DateTime? DataFechamento { get; set; }
    public bool ExisteNoBanco { get; set; }

    public List<ItemBalancoDespesaDTO> Itens { get; set; } = new();

    public decimal Total { get; set; }
    public decimal TotalGeral { get; set; }
    public decimal TotalOficina { get; set; }
    public decimal TotalConcessionaria { get; set; }
}

public class ItemBalancoDespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Setor { get; set; } = "Geral";
    public string? Categoria { get; set; }
    public decimal Valor { get; set; }
    public bool DoModelo { get; set; }
}

public class SalvarItemBalancoDTO
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public Guid? ItemId { get; set; }
    public string Nome { get; set; } = "";
    public string Setor { get; set; } = "Geral";
    public string? Categoria { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>Uma linha do formulário-modelo (entidade Despesa reaproveitada).</summary>
public class ModeloLinhaDespesaDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Setor { get; set; } = "Geral";
    public string? Categoria { get; set; }
    public decimal ValorPadrao { get; set; }
    public bool Ativa { get; set; } = true;
}
