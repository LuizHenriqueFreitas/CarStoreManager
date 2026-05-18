namespace CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;

public class ChecklistPresetDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; }
    public DateTime? DataUltimaAtualizacao { get; set; }
    public List<ChecklistPresetItemDTO> Itens { get; set; } = new();
}

public class ChecklistPresetItemDTO
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = "";
    public int Ordem { get; set; }
}

public class ChecklistPresetLookupDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public int QuantidadeItens { get; set; }
}

public class SalvarChecklistPresetDTO
{
    /// <summary>Vazio/Empty no POST de criação; preenchido no PUT.</summary>
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public List<string> Itens { get; set; } = new();
}
