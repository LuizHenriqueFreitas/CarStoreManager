// Service da entidade VeiculoVenda : Herda da interface IVeiculoVenda.cs

using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class VeiculoVendaService : IVeiculoVendaService
{
    private readonly IVeiculoVendaRepository _repository;

    public VeiculoVendaService(IVeiculoVendaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<VeiculoVendaDTO>> GetByIdAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result<VeiculoVendaDTO>.Fail("Veículo não encontrado");

        return Result<VeiculoVendaDTO>.Ok(VeiculoVendaMapping.ToDto(veiculo));
    }

    public async Task<Result<IEnumerable<VeiculoVendaListaDTO>>> GetAllAsync()
    {
        var veiculos = await _repository.GetAllAsync();
        return Result<IEnumerable<VeiculoVendaListaDTO>>.Ok(
            veiculos.Select(VeiculoVendaMapping.ToListaDto));
    }

    public async Task<Result<IEnumerable<VeiculoVendaListaDTO>>> ObterDisponiveisAsync()
    {
        var veiculos = await _repository.ObterDisponiveisAsync();
        return Result<IEnumerable<VeiculoVendaListaDTO>>.Ok(
            veiculos.Select(VeiculoVendaMapping.ToListaDto));
    }

    public async Task<Result<Guid>> AddAsync(CriarVeiculoVendaDTO dto)
    {
        try
        {
            var veiculo = VeiculoVendaMapping.ToEntity(dto);
            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(veiculo.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar veículo: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(AtualizarVeiculoVendaDTO dto)
    {
        var veiculo = await _repository.GetByIdAsync(dto.Id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            VeiculoVendaMapping.UpdateEntity(veiculo, dto);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

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

        try
        {
            veiculo.MarcarComoDisponivel();
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AtualizarQuilometragemAsync(Guid id, int km)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.AlterarQuilometragem(km);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AdicionarFotoAsync(Guid id, string url)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.AdicionarFoto(url);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> RemoverFotoAsync(Guid id, Guid fotoId)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.RemoverFoto(fotoId);
            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);
        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        _repository.Remove(veiculo);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }
}