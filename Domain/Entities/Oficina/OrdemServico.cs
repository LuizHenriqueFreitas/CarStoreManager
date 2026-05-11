using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Services.Oficina;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Domain.Entities.Oficina;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de OrdemServico.cs

    Esta classe tem testes automaticos implementados para:
        Nada ainda
*/

public class OrdemServico : Entity
{
    public Guid VeiculoClienteId { get; private set; }
    public Guid MecanicoId { get; private set; }
    public Guid ClienteId { get; private set; }

    public TipoServico Tipo { get; private set; }

    public string Descricao { get; private set; } = null!;
    public string NumeroPublico { get; private set; } = null!;

    public DateTime DataCriacao { get; private set; }
    public DateTime PrazoEstimado { get; private set; }

    public Dinheiro CustoServico { get; private set; } = null!;
    public Dinheiro ValorTotal { get; private set; } = null!;

    public StatusOrdemServico Status { get; private set; }

    public List<ItemOrdemServico> Itens { get; private set; } = new();
    public List<ChecklistOrdemServico> Checklist { get; private set; } = new();

    protected OrdemServico() { }

    //o construtor verifica que a descrição nao seja nula e que o prazo seja valido
    public OrdemServico(
        Guid veiculoClienteId,
        Guid mecanicoId,
        Guid clienteId,
        TipoServico tipo,
        string descricao,
        DateTime prazoEstimado,
        decimal custoServico)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição inválida");

        if (prazoEstimado < DateTime.UtcNow)
            throw new ArgumentException("Prazo inválido");

        VeiculoClienteId = veiculoClienteId;
        MecanicoId = mecanicoId;
        ClienteId = clienteId;

        Tipo = tipo;
        Descricao = descricao.Trim();
        PrazoEstimado = prazoEstimado;

        CustoServico = new Dinheiro(custoServico);

        DataCriacao = DateTime.UtcNow;
        Status = StatusOrdemServico.Pendente;
        NumeroPublico = GerarNumeroPublicoAcessoAPI();

        RecalcularTotal();
    }

    /* =====================================
        metodos GETTERS de cada atributo
     =====================================*/
    public Guid GetVeiculoClienteId() => VeiculoClienteId;
    public Guid GetMecacnicoId() => MecanicoId;
    public Guid GetClienteId() => ClienteId;
    public string GetTipoServico() => Tipo.ToString();
    public string GetDescricao() => Descricao;
    public string GetNumeroPublico() => NumeroPublico;
    public DateTime GetDataCriacao() => DataCriacao;
    public DateTime GetPrazoEstimado() => PrazoEstimado;
    public decimal GetCustoServico() => CustoServico.GetValorDinheiro();
    public decimal GetValorTotal() => ValorTotal.GetValorDinheiro();
    public string GetStatus() => Status.ToString();
    public string GetItens()
    {
        
        if (Itens.Count == 0)
        {
            return $"Ordem não possui Itens listados.";
        }
        string temp = "Itens: ";
        for(int i =0; i< Itens.Count; i++)
        {
            temp += Itens[i] + "\n";
        }
        return $"{temp}.";
    }
    public string GetChecklist()
    {
        
        if (Checklist.Count == 0)
        {
            return $"Ordem não possui checklist.";
        }
        string temp = "Checklist: ";
        for(int i =0; i< Checklist.Count; i++)
        {
            temp += Checklist[i] + "\n";
        }
        return $"{temp}.";
    }

    /*  ==================================
        metodos SETTERS para os atributos
      =================================  */

    //definir cliente por Id com verificação que existe esse cadastro
    public void DefinirCliente(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("Cliente inválido");

        ClienteId = clienteId;
    }

    //definir veiculo por Id com verificação que existe esse cadastro
    public void DefinirVeiculo(Guid veiculoClienteId)
    {
        if (veiculoClienteId == Guid.Empty)
            throw new ArgumentException("Veículo inválido");

        VeiculoClienteId = veiculoClienteId;
    }

    //setter do tipo de servico a ser feito com base no enum TipoServico
    public void SetTipo(TipoServico tipo)
    {
        Tipo = tipo;
    }

    /* ===============================================
        Este metodo faz a geração de uma string
        especifica e alearotia para que o cliente
        possa colocar ela na api de busca da OS
        e que assim ele possa verificar a situação
        da ordem de servico pela internet sem ter que
        ligar ou ir até a oficina.
    ================================================ */
    private static string GerarNumeroPublicoAcessoAPI()
    {
        var bytes = Guid.NewGuid().ToByteArray();
        return Convert.ToHexString(bytes)[..8].ToUpper();
    }

    /* ==============================
        Gerenciamento de CHECKLIST
      ==============================*/
    //metodo que faz a criação automatica da checklist padrão
    public void GerarChecklistAutomatico()
    {
        if (Checklist.Any(c => c.Origem == OrigemChecklistItem.Automatico))
            return;

        var itens = ChecklistTemplates.ObterPorTipo(Tipo);
        for (int i = 0; i < itens.Count; i++)
        {
            var item = new ChecklistOrdemServico(Id, itens[i], OrigemChecklistItem.Automatico, i + 1);
            Checklist.Add(item);
        }
    }

    //metodo que adiciona mais itens na checklist padrão já criada
    public void AdicionarItemChecklist(string descricao)
    {
        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar o checklist desta OS");

        var ordem = Checklist.Count + 1;
        var item = new ChecklistOrdemServico(Id, descricao, OrigemChecklistItem.Manual, ordem);
        Checklist.Add(item);
    }

    /*
        Gerenciamento de Itens
    */
    //metodo que adiciona um item necessario para ordem de servico
    public void AdicionarItem(ItemOrdemServico item)
    {
        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar itens em OS finalizada/cancelada");

        Itens.Add(item);
        RecalcularTotal();
    }

    //metodo que remove um item ja colocado na ordem de servico
    public void RemoverItem(Guid itemId)
    {
        var item = Itens.FirstOrDefault(i => i.Id == itemId);

        if (item == null)
            throw new InvalidOperationException("Item não encontrado");

        Itens.Remove(item);
        RecalcularTotal();
    }

    //atualiza um item ja colocado na OS por outro item
    public void AtualizarItem(Guid itemId, int novaQuantidade)
    {
        var item = Itens.FirstOrDefault(i => i.Id == itemId);

        if (item is null)
            throw new InvalidOperationException("Item não encontrado");

        item.AlterarQuantidade(novaQuantidade);

        RecalcularTotal();
    }

    /* ===================================================
        metodo de supote que calcula o valor total
        da Ordem de Servico, a regra adotada é que
        o valor total sera a soma entre o valor 
        total dos itens com o valor de custo do servico
     =====================================================*/

    public void RecalcularTotal()
    {
        var totalItens = Itens
            .Select(i => i.ValorTotal)
            .Aggregate(new Dinheiro(0), (acc, v) => acc.Somar(v));

        ValorTotal = totalItens.Somar(CustoServico);
    }

    /* ==============================================
        Gerenciamento de STATUS da ordem de servico
     ===============================================*/

    /*
        FLUXO DE APROVAÇÃO

        1) Pendente               — recepcionista cria o orçamento (rascunho).
        2) EmAnalise              — recepcionista envia pra mecânico revisar.
        3) AguardandoCliente      — mecânico revisou; recepcionista vai mostrar pro cliente.
        4) Aprovada               — cliente aprovou; OS pronta pra começar.
        5) EmAndamento            — mecânico iniciou o serviço.
        6) Finalizada             — serviço concluído.

        A OS pode ser cancelada a qualquer momento (exceto se já finalizada).
    */

    public void EnviarParaRevisaoMecanico()
    {
        ValidarStatus(StatusOrdemServico.Pendente);
        Status = StatusOrdemServico.EmAnalise;
    }

    public void DevolverParaAjustesDoRecepcionista()
    {
        // mecânico achou que o orçamento precisa de ajustes (volta pra Pendente)
        ValidarStatus(StatusOrdemServico.EmAnalise);
        Status = StatusOrdemServico.Pendente;
    }

    public void AprovarPeloMecanico()
    {
        // mecânico revisou e está OK; agora aguarda o cliente
        ValidarStatus(StatusOrdemServico.EmAnalise);
        Status = StatusOrdemServico.AguardandoCliente;
    }

    public void RegistrarAprovacaoDoCliente()
    {
        ValidarStatus(StatusOrdemServico.AguardandoCliente);
        Status = StatusOrdemServico.Aprovada;
    }

    /*
        metodo que incia uma OS
        a regra é que só pode ser iniciada
        caso o estado anterior seja "Aprovada" (cliente já confirmou)
        ou "Pendente" (fluxo legacy/atalho do admin para começar direto).
    */
    public void Iniciar()
    {
        if (Status != StatusOrdemServico.Aprovada && Status != StatusOrdemServico.Pendente)
            throw new InvalidOperationException(
                "Só é possível iniciar OS aprovada pelo cliente.");
        Status = StatusOrdemServico.EmAndamento;
    }

    /*
        metodo que finaliza uma OS
        a regra é que só pode ser finalizada 
        caso o estado anterior seja "EmAndamento"
    */
    public void Finalizar()
    {
        ValidarStatus(StatusOrdemServico.EmAndamento);
        Status = StatusOrdemServico.Finalizada;
    }

    //metodo para cancelar uma OS - pode ser cancelada a qualquer momento
    public void Cancelar()
    {
        Status = StatusOrdemServico.Cancelada;
    }

    /*
        metodo generico para atualizar senha
        deve bloquear atualizações em OS
        finalizada ou Cancelada.
    */
    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        if (Status == StatusOrdemServico.Finalizada)
            throw new InvalidOperationException("Ordem já finalizada");

        if (Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Ordem cancelada não pode ser alterada");

        Status = novoStatus;
    }

    //metodo que verifica que o status seja igual ao esperado
    private void ValidarStatus(StatusOrdemServico esperado)
    {
        if (Status != esperado)
            throw new InvalidOperationException($"Status inválido: {Status}");
    }
}