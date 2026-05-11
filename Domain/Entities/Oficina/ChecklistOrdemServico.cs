using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de ChecklistOrdemServico.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class ChecklistOrdemServico : Entity
{
    public Guid OrdemServicoId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = null!;
    public StatusChecklistItem Status { get; private set; }
    public OrigemChecklistItem Origem { get; private set; }
    public int OrdemExibicao { get; private set; }

    protected ChecklistOrdemServico() { }

    /*
        Construtor valida que a descrição não seja vazia
        Todo item é gerado no status de pendente
    */
    public ChecklistOrdemServico(
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

    /* ========================================
        REGRAS DE NEGOCIOS Abaixo
        esta classe não tera implementação 
        de getters e setters, visto que só
        é necessaio o construtor e os metodos 
        que mudam os STATUS da checklist
     ==========================================*/

    /*
        metodo para iniciar um item
        necessario que o status anterior seja "Pendente"
    */
    public void IniciarItem()
    {
        if (Status != StatusChecklistItem.Pendente)
            throw new InvalidOperationException("Item já iniciado ou concluído");

        Status = StatusChecklistItem.EmAndamento;
    }

    /*
        metodo para concluir um item
        necessario que o status anterior 
        seja diferente de "Concluido"
    */
    public void ConcluirItem()
    {
        if (Status == StatusChecklistItem.Concluido)
            throw new InvalidOperationException("Item já concluído");

        Status = StatusChecklistItem.Concluido;
    }

    /*
        metodo para atualizar descricao de item
        necessario que a origem do item nao seja "Automatico"
        e que a nova descrição não seja vazia
    */
    public void AtualizarDescricao(string novaDescricao)
    {
        if (Origem == OrigemChecklistItem.Automatico)
            throw new InvalidOperationException("Itens automáticos não podem ser editados");

        if (string.IsNullOrWhiteSpace(novaDescricao))
            throw new ArgumentException("Descrição inválida");

        Descricao = novaDescricao.Trim();
    }
}