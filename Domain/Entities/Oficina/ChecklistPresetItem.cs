using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Oficina;

public class ChecklistPresetItem : Entity
{
    public Guid ChecklistPresetId { get; private set; }
    public string Descricao { get; private set; } = null!;
    public int Ordem { get; private set; }

    protected ChecklistPresetItem() { }

    public ChecklistPresetItem(Guid presetId, string descricao, int ordem)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do item é obrigatória.", nameof(descricao));

        ChecklistPresetId = presetId;
        Descricao = descricao.Trim();
        Ordem = ordem;
    }
}
