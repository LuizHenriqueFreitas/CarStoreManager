using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Veiculo;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repository;

    public VeiculoService(IVeiculoRepository repository)
    {
        _repository = repository;
    }

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<VeiculoDTO>> ObterPorIdAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result<VeiculoDTO>.Fail("Veículo não encontrado");

        return Result<VeiculoDTO>.Ok(
            VeiculoMapping.ToDto(veiculo)
        );
    }

    public async Task<Result<IEnumerable<VeiculoListaDTO>>> ObterTodosAsync()
    {
        var veiculos = await _repository.GetAllAsync();

        var lista = veiculos
            .Select(VeiculoMapping.ToListaDto);

        return Result<IEnumerable<VeiculoListaDTO>>.Ok(lista);
    }

    public async Task<Result<IEnumerable<VeiculoListaDTO>>> ObterDisponiveisAsync()
    {
        var veiculos = await _repository.FindAsync(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel);

        var lista = veiculos
            .Select(VeiculoMapping.ToListaDto);

        return Result<IEnumerable<VeiculoListaDTO>>.Ok(lista);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result> CriarAsync(CriarVeiculoDTO dto)
    {
        if(!Enum.TryParse<EstadoConservacao>(dto.Estado, true, out var estado))
            return Result.Fail("estado inválido");
        try
        {
            var veiculo = new Veiculo(
                Guid.NewGuid(),
                dto.Marca,
                dto.Modelo,
                dto.Cor,
                new Ano(dto.Ano),
                new Quilometragem(dto.Quilometragem),
                new PlacaVeiculo(dto.Placa),
                estado,
                DisponibilidadeVeiculo.Disponivel,
                new Dinheiro(dto.Valor)
            );

            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Erro ao criar veículo: {ex.Message}");
        }
    }

    // =========================
    // ATUALIZAÇÃO
    // =========================

    public async Task<Result> AtualizarAsync(AtualizarVeiculoDTO dto)
    {
        var veiculo = await _repository.GetByIdAsync(dto.Id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        if(!Enum.TryParse<DisponibilidadeVeiculo>(dto.Disponibilidade, true, out var disponibilidade))
            return Result.Fail("disponibilidade invalida");

        if(!Enum.TryParse<EstadoConservacao>(dto.Disponibilidade, true, out var estado))
            return Result.Fail("disponibilidade invalida");
            
        try
        {
            veiculo.AtualizarDados(
                dto.Marca,
                dto.Modelo,
                dto.Cor,
                new Dinheiro(dto.Valor),
                disponibilidade,
                dto.Quilometragem,
                estado
            );

            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // REGRAS DE NEGÓCIO
    // =========================

    public async Task<Result> MarcarComoVendidoAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.MarcarComoVendido();

            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> MarcarComoDisponivelAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        veiculo.MarcarComoDisponivel();

        _repository.Update(veiculo);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> AtualizarQuilometragemAsync(Guid id, int km)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.AtualizarQuilometragem(km);

            _repository.Update(veiculo);
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

    public async Task<Result> RemoverAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        _repository.Remove(veiculo);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}