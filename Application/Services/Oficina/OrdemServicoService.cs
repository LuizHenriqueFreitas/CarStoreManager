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

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<OrdemServicoDTO>> ObterPorIdAsync(Guid id)
    {
        var ordem = await _repository.GetByIdAsync(id);

        if (ordem is null)
            return Result<OrdemServicoDTO>.Fail("Ordem de serviço não encontrada");

        return Result<OrdemServicoDTO>.Ok(
            OrdemServicoMapping.ToDto(ordem)
        );
    }

    public async Task<Result<IEnumerable<OrdemServicoListaDTO>>> ObterTodasAsync()
    {
        var ordens = await _repository.GetAllAsync();

        var lista = ordens
            .Select(OrdemServicoMapping.ToListaDto);

        return Result<IEnumerable<OrdemServicoListaDTO>>.Ok(lista);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result<Guid>> CriarAsync(CriarOrdemServicoDTO dto)
    {
        if (!Enum.TryParse<TipoServico>(dto.Tipo, true, out var tipo))
            return Result<Guid>.Fail("Tipo de serviço inválido");

        try
        {
            var ordem = new OrdemServico(
                dto.VeiculoId,
                dto.MecanicoId,
                dto.ClienteId,
                tipo,
                dto.Descricao,
                dto.PrazoEstimado,
                new Dinheiro(dto.CustoServico)
            );

            ordem.DefinirCliente(dto.ClienteId);
            ordem.DefinirVeiculo(dto.VeiculoId);
            ordem.SetTipo(tipo);

            await _repository.AddAsync(ordem);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(ordem.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar OS: {ex.Message}");
        }
    }

    // =========================
    // ITENS
    // =========================

    public async Task<Result> AdicionarItemAsync(AdicionarItemOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);

        if (ordem is null)
            return Result.Fail("Ordem não encontrada");

        var componente = await _componenteRepository.GetByIdAsync(dto.ComponenteId);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            // valida estoque
            if (componente.QuantidadeEstoque < dto.Quantidade)
                return Result.Fail("Estoque insuficiente");

            var item = new ItemOrdemServico(
                dto.ComponenteId,
                dto.OrdemServicoId,
                dto.Quantidade,
                componente.Valor
            );

            ordem.AdicionarItem(item);

            // baixa estoque
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
            return Result.Fail("Ordem não encontrada");

        try
        {
            ordem.RemoverItem(itemId);

            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AtualizarItemAsync(AtualizarItemOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.OrdemServicoId);

        if (ordem is null)
            return Result.Fail("Ordem não encontrada");

        try
        {
            ordem.AtualizarItem(dto.ItemId, dto.NovaQuantidade);

            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // STATUS
    // =========================

    public async Task<Result> AtualizarStatusAsync(AtualizarOrdemServicoDTO dto)
    {
        var ordem = await _repository.GetByIdAsync(dto.Id);

        if (ordem is null)
            return Result.Fail("Ordem não encontrada");

        if (!Enum.TryParse<StatusOrdemServico>(dto.Status, true, out var status))
            return Result.Fail("Status inválido");

        try
        {
            ordem.AtualizarStatus(status);

            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // CÁLCULOS
    // =========================

    public async Task<Result> RecalcularValoresAsync(Guid ordemId)
    {
        var ordem = await _repository.GetByIdAsync(ordemId);

        if (ordem is null)
            return Result.Fail("Ordem não encontrada");

        try
        {
            ordem.RecalcularTotal();

            _repository.Update(ordem);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }
}