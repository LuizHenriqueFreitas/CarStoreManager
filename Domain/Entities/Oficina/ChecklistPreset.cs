using CarStoreManager.Domain.Base;

namespace CarStoreManager.Domain.Entities.Oficina;

/// <summary>
/// Modelo de checklist editável pelo admin na tela de configurações.
/// Pode haver múltiplos presets (ex.: "Padrão", "Carro Elétrico", "Revisão Rápida")
/// e o recepcionista escolhe qual usar na criação da OS — os itens são copiados
/// como snapshot, então alterações futuras no preset não afetam OS existentes.
/// </summary>
public class ChecklistPreset : Entity
{
    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; }
    public DateTime? DataUltimaAtualizacao { get; private set; }

    public List<ChecklistPresetItem> Itens { get; private set; } = new();

    protected ChecklistPreset() { }

    public ChecklistPreset(string nome)
    {
        AtualizarNome(nome);
        Ativo = true;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do preset é obrigatório.", nameof(nome));

        Nome = nome.Trim();
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void AdicionarItem(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do item é obrigatória.", nameof(descricao));

        var ordem = Itens.Count == 0 ? 1 : Itens.Max(i => i.Ordem) + 1;
        Itens.Add(new ChecklistPresetItem(Id, descricao, ordem));
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Substitui completamente a lista de itens (uso típico: o admin reorganiza
    /// o preset inteiro em um único POST/PUT). As ordens são reatribuídas
    /// sequencialmente seguindo a ordem da lista recebida.
    /// </summary>
    public void SubstituirItens(IEnumerable<string> descricoes)
    {
        Itens.Clear();
        var i = 1;
        foreach (var d in descricoes)
        {
            if (string.IsNullOrWhiteSpace(d)) continue;
            Itens.Add(new ChecklistPresetItem(Id, d, i++));
        }
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DataUltimaAtualizacao = DateTime.UtcNow;
    }
}
