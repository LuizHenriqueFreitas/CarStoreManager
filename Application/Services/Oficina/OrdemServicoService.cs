using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;
    private readonly IComponenteRepository _componenteRepository;

    public OrdemServicoService(
        IOrdemServicoRepository repository,
        IComponenteRepository componenteRepository)
    {
        _repository = repository;
        _componenteRepository = componenteRepository;
    }

    public async Task<Result<OrdemServicoDTO>> GetByIdAsync(Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);

        if (ordem is null)
            return Result<OrdemServicoDTO>.Fail("OS não encontrada");

        return Result<OrdemServicoDTO>.Ok(OrdemServicoMapping.ToDto(ordem));
    }

    public async Task<Result<IEnumerable<OrdemServicoListaDTO>>> GetAllAsync()
    {
        var lista = (await _repository.GetAllAsync())
            .Select(OrdemServicoMapping.ToListaDto);

        return Result<IEnumerable<OrdemServicoListaDTO>>.Ok(lista);
    }

    public async Task<Result<OrdemServicoPublicaDTO>> ObterPorNumeroPublicoAsync(string numero)
    {
        var ordem = await _repository.ObterPorNumeroPublicoAsync(numero);

        if (ordem is null)
            return Result<OrdemServicoPublicaDTO>.Fail("OS não encontrada");

        return Result<OrdemServicoPublicaDTO>.Ok(
            OrdemServicoMapping.ToPublicaDto(ordem)
        );
    }

    public async Task<Result<Guid>> AddAsync(CriarOrdemServicoDTO dto)
    {
        try
        {
            var ordem = OrdemServicoMapping.ToEntity(dto);

            ordem.GerarChecklistAutomatico();

            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(ordem.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail(ex.Message);
        }
    }

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
                componente.Valor
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

    public async Task<Result> UpdateAsync (AtualizarOrdemServicoDTO entity)
    {
        return Result.Fail("Funcao nao implementada");
    }

    public async Task<Result> RemoveAsync (Guid id)
    {
        return Result.Fail("Funcao nao implementada");
    }
}