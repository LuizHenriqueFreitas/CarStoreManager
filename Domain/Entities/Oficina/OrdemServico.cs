using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;
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

    /// <summary>
    /// Status anterior preservado para suportar retornos de pausas
    /// (BuscandoPecasParaOrcamento volta para Pendente/EmAnalise; Pausada volta para EmAndamento).
    /// </summary>
    public StatusOrdemServico? StatusAnterior { get; private set; }

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
    /// <summary>
    /// Gera o checklist inicial a partir de uma lista de descrições vinda do
    /// preset escolhido pelo recepcionista na criação da OS. É um snapshot —
    /// edições posteriores no preset não afetam essa OS.
    /// </summary>
    public void GerarChecklistAPartirDoPreset(IEnumerable<string> descricoes)
    {
        if (Checklist.Any(c => c.Origem == OrigemChecklistItem.Automatico))
            return;

        var lista = descricoes?.Where(d => !string.IsNullOrWhiteSpace(d)).ToList() ?? new();
        for (int i = 0; i < lista.Count; i++)
        {
            var item = new ChecklistOrdemServico(Id, lista[i], OrigemChecklistItem.Automatico, i + 1);
            Checklist.Add(item);
        }
    }

    //metodo que adiciona mais itens na checklist padrão já criada
    public void AdicionarItemChecklist(string descricao)
    {
        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar o checklist desta OS");

        var ordem = Checklist.Count == 0 ? 1 : Checklist.Max(c => c.OrdemExibicao) + 1;
        var item = new ChecklistOrdemServico(Id, descricao, OrigemChecklistItem.Manual, ordem);
        Checklist.Add(item);
    }

    /// <summary>
    /// Remove qualquer item do checklist (vindo do preset ou manual) — o
    /// mecânico tem controle total sobre os itens da OS dele.
    /// </summary>
    public void RemoverItemChecklist(Guid itemId)
    {
        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar o checklist desta OS");

        var item = Checklist.FirstOrDefault(c => c.Id == itemId)
            ?? throw new InvalidOperationException("Item do checklist não encontrado");

        Checklist.Remove(item);
    }

    /*
        Gerenciamento de Itens
    */
    //metodo que adiciona um item necessario para ordem de servico
    public void AdicionarItem(ItemOrdemServico item)
    {
        if (Status is StatusOrdemServico.Finalizada
                   or StatusOrdemServico.Entregue
                   or StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar itens em OS finalizada/entregue/cancelada");

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
        metodo que finaliza uma OS — significa apenas que o mecânico terminou
        o trabalho técnico. A cobrança e a entrega ao cliente são responsabilidade
        da recepção (ver Entregar()). Só é possível finalizar a partir de EmAndamento.
    */
    public void Finalizar()
    {
        ValidarStatus(StatusOrdemServico.EmAndamento);
        Status = StatusOrdemServico.Finalizada;
    }

    /*
        Recepcionista marca a OS como entregue ao cliente — terminal feliz.
        A regra de "tem que estar totalmente paga" é validada no service
        (a entidade não conhece pagamentos). Só é possível entregar a partir de Finalizada.
    */
    public void Entregar()
    {
        ValidarStatus(StatusOrdemServico.Finalizada);
        Status = StatusOrdemServico.Entregue;
    }

    //metodo para cancelar uma OS - pode ser cancelada a qualquer momento
    public void Cancelar()
    {
        Status = StatusOrdemServico.Cancelada;
    }

    /// <summary>
    /// Mecânico abriu requisição de peça inexistente — pausa o orçamento até
    /// admin atender. Permitido apenas em Pendente ou EmAnalise (o orçamento
    /// ainda está sendo montado).
    /// </summary>
    public void MarcarComoBuscandoPecas()
    {
        if (Status is StatusOrdemServico.Finalizada or StatusOrdemServico.Cancelada
            or StatusOrdemServico.BuscandoPecasParaOrcamento)
            return;
        if (Status is not (StatusOrdemServico.Pendente or StatusOrdemServico.EmAnalise))
            throw new InvalidOperationException(
                $"Só é possível requisitar peças durante a montagem do orçamento (atual: {Status}).");

        StatusAnterior = Status;
        Status = StatusOrdemServico.BuscandoPecasParaOrcamento;
    }

    /// <summary>
    /// Admin atendeu todas as requisições — devolve a OS ao status anterior
    /// (Pendente ou EmAnalise) para o orçamento prosseguir.
    /// </summary>
    public void RetomarAposBuscaDePecas()
    {
        if (Status != StatusOrdemServico.BuscandoPecasParaOrcamento)
            throw new InvalidOperationException(
                $"OS não está em busca de peças (atual: {Status}).");
        Status = StatusAnterior ?? StatusOrdemServico.Pendente;
        StatusAnterior = null;
    }

    /// <summary>
    /// Mecânico emite alerta durante a execução — pausa para o cliente decidir
    /// sobre escopo aumentado. Permitido apenas em EmAndamento.
    /// </summary>
    public void Pausar()
    {
        if (Status != StatusOrdemServico.EmAndamento)
            throw new InvalidOperationException(
                $"Só é possível pausar OS em andamento (atual: {Status}).");

        StatusAnterior = Status;
        Status = StatusOrdemServico.Pausada;
    }

    /// <summary>
    /// Cliente decidiu (aprovou ou recusou novo orçamento) — OS volta para
    /// EmAndamento independentemente da decisão. A diferença é se o admin
    /// adicionou novos itens/custos antes de retomar.
    /// </summary>
    public void Retomar()
    {
        if (Status != StatusOrdemServico.Pausada)
            throw new InvalidOperationException(
                $"Só é possível retomar OS pausada (atual: {Status}).");
        Status = StatusAnterior ?? StatusOrdemServico.EmAndamento;
        StatusAnterior = null;
    }

    /*
        metodo generico para atualizar senha
        deve bloquear atualizações em OS
        finalizada ou Cancelada.
    */
    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        if (Status == StatusOrdemServico.Entregue)
            throw new InvalidOperationException("Ordem já entregue");

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