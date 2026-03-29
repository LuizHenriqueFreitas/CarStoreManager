//classe que armaazena os recursos da checklist de itens que vai ser usada

using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

public class ChecklistItem : Entity
{
    public Guid OrdemServicoId { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public StatusChecklistItem Status { get; private set; }
    public OrigemChecklistItem Origem { get; private set; }
    public int OrdemExibicao { get; private set; }

    protected ChecklistItem() { }

    public ChecklistItem(
        Guid ordemServicoId,
        string descricao,
        OrigemChecklistItem origem,
        int ordemExibicao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição do item inválida");

        OrdemServicoId = ordemServicoId;
        Descricao = descricao.Trim();
        Origem = origem;
        OrdemExibicao = ordemExibicao;
        Status = StatusChecklistItem.Pendente;
    }

    public void IniciarItem()
    {
        if (Status != StatusChecklistItem.Pendente)
            throw new InvalidOperationException("Item já iniciado ou concluído");

        Status = StatusChecklistItem.EmAndamento;
    }

    public void ConcluirItem()
    {
        if (Status == StatusChecklistItem.Concluido)
            throw new InvalidOperationException("Item já concluído");

        Status = StatusChecklistItem.Concluido;
    }

    public void AtualizarDescricao(string novaDescricao)
    {
        if (Origem == OrigemChecklistItem.Automatico)
            throw new InvalidOperationException("Itens automáticos não podem ser editados");

        if (string.IsNullOrWhiteSpace(novaDescricao))
            throw new ArgumentException("Descrição inválida");

        Descricao = novaDescricao.Trim();
    }
}