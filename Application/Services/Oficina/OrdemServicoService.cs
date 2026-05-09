using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
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

    public OrdemServicoService(
        IOrdemServicoRepository repository,
        IComponenteRepository componenteRepository,
        IMecanicoService mecanicoService)
    {
        _repository = repository;
        _componenteRepository = componenteRepository;
        _mecanicoService = mecanicoService;
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
                var componente = await _componenteRepository.GetByIdAsync(itemDto.ComponenteId);
                if (componente != null)
                {
                    ordem.AdicionarItem(new ItemOrdemServico(
                        itemDto.Id,
                        itemDto.ComponenteId,
                        itemDto.Quantidade,
                        itemDto.ValorUnitario
                    ));
                }
            }

            ordem.GerarChecklistAutomatico();

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
        metodo para adicionar novos itens na ordemServico,
        falha caso os componente ou a ordem seja vazio,
        caso não tenha estoque suficiente do componente retorna um aviso
    */
    public async Task<Result> AdicionarItemAsync(AdicionarItemOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);
        var componente = await _componenteRepository.GetByIdAsync(dto.ComponenteId);

        if (ordem is null || componente is null)
            return Result.Fail("Dados inválidos");

        if (componente.QuantidadeEstoque < dto.Quantidade)
            return Result.Fail("Estoque insuficiente");

        try
        {
            var item = new ItemOrdemServico(
                dto.ComponenteId,
                dto.OrdemServicoId,
                dto.Quantidade,
                componente.GetValor()
            );

            ordem.AdicionarItem(item);
            componente.RemoverEstoque(dto.Quantidade);

            _repository.Update(ordem);
            _componenteRepository.Update(componente);
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

        ordem.RemoverItem(itemId);

        _repository.Update(ordem);
        await _repository.SaveChangesAsync();

        return Result.Ok();
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

        ordem.AdicionarItemChecklist(dto.Descricao);

        _repository.Update(ordem);
        await _repository.SaveChangesAsync();

        return Result.Ok();
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

    /*
        metodo que remove OrdemServico por id
        caso seja vazia retorna erro
    */    
    public async Task<Result> RemoveAsync (Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);

        if (ordem is null)
            return Result.Fail("Mecânico não encontrado");

        _repository.Remove(ordem);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}