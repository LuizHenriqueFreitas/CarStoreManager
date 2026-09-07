using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de OrdemServicoService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IComponenteRepository _componenteRepository;
    private readonly IMecanicoService _mecanicoService;

    private readonly IPagamentoOrdemServicoRepository? _pagamentoRepo;
    private readonly IAlertaOSRepository? _alertaRepo;
    private readonly IConfiguracaoSistemaRepository? _configRepo;
    private readonly IChecklistPresetRepository? _presetRepo;

    public OrdemServicoService(
        IOrdemServicoRepository repository,
        IComponenteRepository componenteRepository,
        IMecanicoService mecanicoService,
        IPagamentoOrdemServicoRepository? pagamentoRepo = null,
        IAlertaOSRepository? alertaRepo = null,
        IConfiguracaoSistemaRepository? configRepo = null,
        IChecklistPresetRepository? presetRepo = null)
    {
        _repository = repository;
        _componenteRepository = componenteRepository;
        _mecanicoService = mecanicoService;
        _pagamentoRepo = pagamentoRepo;
        _alertaRepo = alertaRepo;
        _configRepo = configRepo;
        _presetRepo = presetRepo;
    }

    /*
        metodo de busca por id valida que
        caso ordemServico buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<OrdemServicoDTO>> GetByIdAsync(Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);

        if (ordem is null)
            return Result<OrdemServicoDTO>.Fail("OS não encontrada");

        return Result<OrdemServicoDTO>.Ok(OrdemServicoMapping.ToDto(ordem));
    }

    //buscar todas as ordemServico
    public async Task<Result<IEnumerable<OrdemServicoListaDTO>>> GetAllAsync()
    {
        var lista = (await _repository.GetAllAsync())
            .Select(OrdemServicoMapping.ToListaDto);

        return Result<IEnumerable<OrdemServicoListaDTO>>.Ok(lista);
    }

    /*
        metodo que busca uma ordemServico pelo numero publico de acesso
        caso seja vazia retorna que não foi encontrada
    */
    public async Task<Result<OrdemServicoPublicaDTO>> ObterPorNumeroPublicoAsync(string numero)
    {
        var ordem = await _repository.ObterPorNumeroPublicoAsync(numero);

        if (ordem is null)
            return Result<OrdemServicoPublicaDTO>.Fail("OS não encontrada");

        return Result<OrdemServicoPublicaDTO>.Ok(
            OrdemServicoMapping.ToPublicaDto(ordem)
        );
    }

    /*
        metodo que adiciona novas ordemServico 
        todas tem como padrao a checklist automatica embutida
        - tambem atualiza a ocupação do mecanico pelo
        mecanico service
    */
    public async Task<Result<Guid>> AddAsync(CriarOrdemServicoDTO dto)
    {
        try
        {
            var ordem = OrdemServicoMapping.ToEntity(dto);

            await _mecanicoService.AtualizarOcupacaoAsync(dto.MecanicoId);
            
            foreach (var itemDto in dto.Itens)
            {
                // Recepção só pode escolher Estoque ou Cliente — Encomenda
                // só nasce via fluxo de RequisicaoPeca atendida.
                var origem = OrigemItemOrdemServico.Estoque;
                if (!string.IsNullOrWhiteSpace(itemDto.Origem) &&
                    Enum.TryParse<OrigemItemOrdemServico>(itemDto.Origem, true, out var parsed) &&
                    parsed != OrigemItemOrdemServico.Encomenda)
                {
                    origem = parsed;
                }

                if (origem == OrigemItemOrdemServico.Cliente)
                {
                    // Peça trazida pelo cliente: sem catálogo, sem estoque, sem
                    // valor — só o nome digitado na hora, pra registro.
                    if (string.IsNullOrWhiteSpace(itemDto.DescricaoLivre)) continue;
                    ordem.AdicionarItem(new ItemOrdemServico(ordem.Id, itemDto.Quantidade, itemDto.DescricaoLivre));
                    continue;
                }

                if (itemDto.ComponenteId is not Guid componenteId) continue;
                var componente = await _componenteRepository.GetByIdAsync(componenteId);
                if (componente != null)
                {
                    ordem.AdicionarItem(new ItemOrdemServico(
                        componenteId,
                        ordem.Id,
                        itemDto.Quantidade,
                        itemDto.ValorUnitario,
                        origem
                    ));
                }
            }

            // Aplica preset escolhido pela recepção como snapshot inicial do
            // checklist da OS. Sem preset => OS começa sem checklist e o
            // mecânico monta do zero adicionando itens.
            if (dto.ChecklistPresetId.HasValue && _presetRepo is not null)
            {
                var preset = await _presetRepo.GetByIdAsync(dto.ChecklistPresetId.Value);
                if (preset is not null && preset.Ativo)
                {
                    var descricoes = preset.Itens
                        .OrderBy(i => i.Ordem)
                        .Select(i => i.Descricao);
                    ordem.GerarChecklistAPartirDoPreset(descricoes);
                }
            }

            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(ordem.Id);
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null)
                Console.WriteLine($"[ERRO OS] Inner: {ex.InnerException.Message}");
            return Result<Guid>.Fail($"Erro ao criar ordem de serviço: {ex.Message}");
        }
    }

    /*
        metodo para adicionar novos itens na ordemServico.
        Regras:
        - O valor unitário SEMPRE vem do ValorVenda do componente (custo × margem).
          O mecânico nunca digita preço; o DTO.ValorUnitario é ignorado.
        - Se a OS já está EmAndamento, adicionar item caracteriza aumento de escopo:
          emite AlertaOS automaticamente, pausa a OS e exige reaprovação do cliente.
    */
    public async Task<Result> AdicionarItemAsync(AdicionarItemOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);
        if (ordem is null)
            return Result.Fail("Dados inválidos");

        if (!Enum.TryParse<Domain.Enums.OrigemItemOrdemServico>(dto.Origem, true, out var origem))
            return Result.Fail($"Origem inválida: {dto.Origem}. Use Estoque ou Cliente.");

        // Encomenda só é criada via fluxo de RequisicaoPeca atendida pelo admin
        // — não pode ser informada manualmente na adição de item.
        if (origem == Domain.Enums.OrigemItemOrdemServico.Encomenda)
            return Result.Fail("Itens por encomenda só podem ser adicionados via requisição atendida pelo admin.");

        try
        {
            ItemOrdemServico item;
            string descricaoAlerta;
            decimal valorUnit = 0;

            if (origem == Domain.Enums.OrigemItemOrdemServico.Cliente)
            {
                // Peça trazida pelo cliente: sem catálogo, sem estoque, sem valor
                // — só o nome digitado na hora, pra registro.
                if (string.IsNullOrWhiteSpace(dto.DescricaoLivre))
                    return Result.Fail("Informe o nome da peça trazida pelo cliente.");

                item = new ItemOrdemServico(dto.OrdemServicoId, dto.Quantidade, dto.DescricaoLivre);
                descricaoAlerta = $"Novo item trazido pelo cliente adicionado durante o serviço: " +
                    $"{item.DescricaoLivre} ({dto.Quantidade}×). OS pausada — aguardando reaprovação do cliente.";
            }
            else
            {
                var componente = await _componenteRepository.GetByIdAsync(dto.ComponenteId);
                if (componente is null)
                    return Result.Fail("Componente não encontrado.");

                // Valor unitário SEMPRE vem do componente — mecânico não tem autonomia
                // pra precificar peças (ver memory: política da oficina).
                valorUnit = componente.ValorVenda;
                item = new ItemOrdemServico(dto.ComponenteId, dto.OrdemServicoId, dto.Quantidade, valorUnit, origem);
                descricaoAlerta =
                    $"Novo componente adicionado durante o serviço: {componente.Nome} " +
                    $"({dto.Quantidade}× R$ {valorUnit:N2} = R$ {(valorUnit * dto.Quantidade):N2}). " +
                    $"Novo valor total da OS: R$ {ordem.GetValorTotal():N2}. " +
                    "OS pausada — aguardando reaprovação do cliente.";
            }

            // Se a OS já está em andamento, isso é aumento de escopo — pausa e alerta
            // o cliente. O alerta precisa ser criado ANTES de Pausar(), porque Pausar()
            // só funciona em EmAndamento (a entidade muda o status).
            var precisaAlertar = ordem.Status == StatusOrdemServico.EmAndamento;

            ordem.AdicionarItem(item);

            if (precisaAlertar)
            {
                ordem.Pausar();

                if (_alertaRepo is not null)
                {
                    var alerta = new AlertaOS(ordem.Id, ordem.GetMecacnicoId(), descricaoAlerta);
                    await _alertaRepo.AddAsync(alerta);
                }
            }

            // Registra o item explicitamente no DbSet — evita o caso em que o
            // change tracker não detecta o Add via collection com OwnedTypes
            // e tenta UPDATE numa linha inexistente.
            await _repository.AdicionarItemAsync(item);
            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que remove itens ja adicionados na ordemServico
        caso a ordem seja vazia é retornado um aviso
    */
    public async Task<Result> RemoverItemAsync(Guid ordemId, Guid itemId)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);

        if (ordem is null)
            return Result.Fail("OS não encontrada");

        try
        {
            ordem.RemoverItem(itemId);

            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível remover o item da ordem de serviço. Tente novamente em instantes.");
        }
    }

    /*
        metodo que atualiza um item ja adicionado
        na ordemServico, pode trocar por outro item 
        ou apenas alterar a quantidade necessaria
        caso a ordem seja vazia retorna um aviso
    */
    public async Task<Result> AtualizarItemAsync(AtualizarItemOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);

        if (ordem is null)
            return Result.Fail("OS não encontrada");

        ordem.AtualizarItem(dto.ItemId, dto.NovaQuantidade);

        _repository.Update(ordem);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    /*
        metodo para atualizar o status da ordem,
        valida que a ordem exista, caso contrario falha
        tambem verifica que o status seja permitido
        de acordo com o enum StatusOrdemServico.cs
    */
    public async Task<Result> AtualizarStatusAsync(AtualizarOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.Id);

        if (ordem is null)
            return Result.Fail("OS não encontrada");

        if (!Enum.TryParse<StatusOrdemServico>(dto.Status, true, out var status))
            return Result.Fail("Status inválido");

        ordem.AtualizarStatus(status);

        _repository.Update(ordem);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    /*
        metodo que adiciona novos itens na checklist
        falha caso a ordem seja vazia
    */
    public async Task<Result> AdicionarItemChecklistAsync(AdicionarChecklistItemDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);

        if (ordem is null)
            return Result.Fail("OS não encontrada");

        try
        {
            ordem.AdicionarItemChecklist(dto.Descricao);

            // Mesmo padrão de AdicionarItemAsync — registra explicitamente para
            // o EF tratar como Add no DbSet ao invés de tentar Update no pai.
            var itemNovo = ordem.Checklist[^1];
            await _repository.AdicionarItemChecklistAsync(itemNovo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /// <summary>
    /// Remove um item do checklist da OS — válido para itens de qualquer origem
    /// (preset ou manuais), permitindo ao mecânico curar a lista.
    /// </summary>
    public async Task<Result> RemoverItemChecklistAsync(Guid ordemId, Guid itemId)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);
        if (ordem is null) return Result.Fail("OS não encontrada");

        try
        {
            ordem.RemoverItemChecklist(itemId);
            _repository.Update(ordem);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /// <summary>
    /// Atualiza a descrição de um item do checklist — permitido para qualquer
    /// origem (preset ou manual), o mecânico tem controle total.
    /// </summary>
    public async Task<Result> AtualizarDescricaoChecklistAsync(Guid ordemId, Guid itemId, string novaDescricao)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);
        var item = ordem?.Checklist.FirstOrDefault(c => c.Id == itemId);
        if (item is null) return Result.Fail("Item do checklist não encontrado");

        try
        {
            item.AtualizarDescricao(novaDescricao);
            _repository.Update(ordem!);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /*
        metodo que atualiza o status de item da checklist
        falha caso o item seja vazio
        falha caso status informado seja invalido
    */
    public async Task<Result> AtualizarStatusChecklistAsync(AtualizarStatusChecklistDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);

        var item = ordem?.Checklist.FirstOrDefault(c => c.Id == dto.ItemId);

        if (item is null)
            return Result.Fail("Item não encontrado");

        // need_to_do.txt: checklist só é marcável na etapa 5 (serviço em andamento).
        try { ordem!.GarantirChecklistEditavel(); }
        catch (InvalidOperationException ex) { return Result.Fail(ex.Message); }

        if (dto.NovoStatus == "EmAndamento") item.IniciarItem();
        else if (dto.NovoStatus == "Concluido") item.ConcluirItem();
        else return Result.Fail("Status inválido");

        _repository.Update(ordem!);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    /*
        metodo que recalcula valor total por meio do metodo interno de 
        OrdemServico, falha caso a ordem seja vazia
    */
    public async Task<Result> RecalcularValoresAsync(Guid ordemId)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);

        if (ordem is null)
            return Result.Fail("OS não encontrada");

        ordem.RecalcularTotal();

        _repository.Update(ordem);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    /*
        ainda nao foi implementada uma maneira de atualizar
        as informações internas de uma OS
    */
    public async Task<Result> UpdateAsync (AtualizarOrdemServicoDTO entity)
    {
        return Result.Fail("Funcao nao implementada");
    }

    // =========================
    // FLUXO DE APROVAÇÃO
    // =========================

    public Task<Result> EnviarParaRevisaoAsync(Guid ordemId)
        => MutarOrdem(ordemId, o => o.EnviarParaRevisaoMecanico());

    public Task<Result> AprovarPeloMecanicoAsync(Guid ordemId)
        => MutarOrdem(ordemId, o => o.AprovarPeloMecanico());

    public Task<Result> DevolverParaAjustesAsync(Guid ordemId)
        => MutarOrdem(ordemId, o => o.DevolverParaAjustesDoRecepcionista());

    public Task<Result> RegistrarAprovacaoDoClienteAsync(Guid ordemId)
        => MutarOrdem(ordemId, o => o.RegistrarAprovacaoDoCliente());

    public async Task<Result> IniciarAsync(Guid ordemId)
    {
        // Modo operante: se a configuração exige entrada mínima, valida o
        // percentual já pago antes de permitir a transição para EmAndamento.
        if (_configRepo is not null && _pagamentoRepo is not null)
        {
            var cfg = await _configRepo.ObterAsync();
            if (cfg.ExigirEntradaMinima && cfg.PercentualEntradaMinima > 0m)
            {
                var ordem = await _repository.GetByIdAsync(ordemId);
                if (ordem is null) return Result.Fail("Ordem de serviço não encontrada");

                var total = ordem.GetValorTotal();
                if (total > 0m)
                {
                    var pagamentos = await _pagamentoRepo.ObterPorOrdemAsync(ordemId);
                    var pago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
                    var percentualPago = (pago / total) * 100m;

                    if (percentualPago < cfg.PercentualEntradaMinima)
                    {
                        var entradaExigida = total * (cfg.PercentualEntradaMinima / 100m);
                        var faltando = entradaExigida - pago;
                        return Result.Fail(
                            $"Entrada mínima de {cfg.PercentualEntradaMinima:N2}% não atingida. " +
                            $"Pago: R$ {pago:N2} de R$ {total:N2} ({percentualPago:N2}%). " +
                            $"Receba ao menos R$ {faltando:N2} para iniciar o serviço.");
                    }
                }
            }
        }

        return await MutarOrdem(ordemId, o => o.Iniciar());
    }

    /// <summary>
    /// Mecânico declara que terminou o serviço técnico. NÃO exige pagamento —
    /// a OS vai para Finalizada e fica aguardando a recepção cobrar e entregar
    /// (EntregarAsync). Gera NF de venda automaticamente nesse momento.
    /// </summary>
    public async Task<Result> FinalizarAsync(Guid ordemId)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);
        if (ordem is null) return Result.Fail("Ordem de serviço não encontrada");

        try
        {
            ordem.Finalizar();

            // Se a OS já nasceu totalmente paga (ex.: entrada mínima cobriu o
            // valor todo), confirma na hora — sem isso ela ficaria presa em
            // "Pagamento Pendente" com saldo zerado, exigindo um pagamento de
            // R$ 0,00 pra sair do estado.
            if (_pagamentoRepo is not null)
            {
                var pagamentos = await _pagamentoRepo.ObterPorOrdemAsync(ordemId);
                var pago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
                if (pago >= ordem.GetValorTotal())
                    ordem.ConfirmarPagamentoCompleto();
            }

            _repository.Update(ordem);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    /// <summary>
    /// Recepção cobrou e entrega a OS ao cliente. Bloqueia se ainda houver
    /// saldo a pagar. Estado terminal feliz: StatusOrdemServico.Entregue.
    /// </summary>
    public async Task<Result> EntregarAsync(Guid ordemId)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);
        if (ordem is null) return Result.Fail("Ordem de serviço não encontrada");

        if (_pagamentoRepo is not null)
        {
            var pagamentos = await _pagamentoRepo.ObterPorOrdemAsync(ordemId);
            var pago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
            var total = ordem.GetValorTotal();
            if (pago < total)
            {
                var restante = total - pago;
                return Result.Fail(
                    $"Pagamento pendente: faltam R$ {restante:N2} de R$ {total:N2}. " +
                    "Receba o pagamento antes de entregar o veículo ao cliente.");
            }
        }

        try
        {
            ordem.Entregar();
            _repository.Update(ordem);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(ex.Message); }
    }

    public Task<Result> CancelarAsync(Guid ordemId)
        => MutarOrdem(ordemId, o => o.Cancelar());

    private async Task<Result> MutarOrdem(Guid id, Action<Domain.Entities.Oficina.OrdemServico> mutacao)
    {
        var ordem = await _repository.GetByIdAsync(id);
        if (ordem is null)
            return Result.Fail("Ordem de serviço não encontrada");

        try
        {
            mutacao(ordem);
            _repository.Update(ordem);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro: {ex.Message}");
        }
    }

    /*
        metodo que remove OrdemServico por id
        caso seja vazia retorna erro
    */    
    public async Task<Result> RemoveAsync (Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);

        if (ordem is null)
            return Result.Fail("Ordem de serviço não encontrada");

        try
        {
            _repository.Remove(ordem);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível excluir a ordem de serviço. Ela pode ter pagamentos ou peças vinculadas.");
        }
    }
}