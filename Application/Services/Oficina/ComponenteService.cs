using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class ComponenteService : IComponenteService
{
    private readonly IComponenteRepository _repository;

    public ComponenteService(IComponenteRepository repository)
    {
        _repository = repository;
    }

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<ComponenteDTO>> GetByIdAsync(Guid id)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result<ComponenteDTO>.Fail("Componente não encontrado");

        return Result<ComponenteDTO>.Ok(
            ComponenteMapping.ToDto(componente)
        );
    }

    public async Task<Result<IEnumerable<ComponenteListaDTO>>> GetAllAsync()
    {
        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Select(ComponenteMapping.ToListaDto);

        return Result<IEnumerable<ComponenteListaDTO>>.Ok(lista);
    }

    public async Task<Result<IEnumerable<ComponenteListaDTO>>> ObterComEstoqueBaixoAsync()
    {
        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Where(c => c.EstoqueBaixo())
            .Select(ComponenteMapping.ToListaDto);

        return Result<IEnumerable<ComponenteListaDTO>>.Ok(lista);
    }

    public async Task<Result<IEnumerable<ComponenteLookupDTO>>> ObterPorSistemaAsync(string sistema)
    {
        if (!Enum.TryParse<SistemaComponente>(sistema, true, out var sistemaEnum))
            return Result<IEnumerable<ComponenteLookupDTO>>.Fail("Sistema inválido");

        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Where(c => c.Sistema == sistemaEnum)
            .Select(ComponenteMapping.ToLookupDto);

        return Result<IEnumerable<ComponenteLookupDTO>>.Ok(lista);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result<Guid>> AddAsync(CriarComponenteDTO dto)
    {
        if(!Enum.TryParse<SistemaComponente>(dto.Sistema, true, out var sistema))
            return Result<Guid>.Fail("Sistema inválido");
        try
        {
            var componente = new Componente(
                dto.Nome,
                dto.Modelo,
                sistema,
                new Dinheiro(dto.Valor),
                dto.QuantidadeEstoque,
                dto.EstoqueMinimo
            );

            await _repository.AddAsync(componente);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(componente.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar componente: {ex.Message}");
        }
    }

    // =========================
    // ATUALIZAÇÃO
    // =========================

    public async Task<Result> UpdateAsync(AtualizarComponenteDTO dto)
    {
        var componente = await _repository.GetByIdAsync(dto.Id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            componente.AtualizarDados(
                new Dinheiro(dto.Valor),
                dto.QuantidadeEstoque,
                dto.EstoqueMinimo
            );

            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // ESTOQUE (MUITO IMPORTANTE)
    // =========================

    public async Task<Result> EntradaEstoqueAsync(Guid id, int quantidade)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            componente.AdicionarEstoque(quantidade);

            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> SaidaEstoqueAsync(Guid id, int quantidade)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            componente.RemoverEstoque(quantidade);

            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // REMOÇÃO
    // =========================

    public async Task<Result> RemoveAsync(Guid id)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        _repository.Remove(componente);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}