using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.GerenteVendas;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Concessionaria;

public class GerenteVendasService : IGerenteVendasService
{
    private readonly IGerenteVendasRepository _repository;

    public GerenteVendasService(IGerenteVendasRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GerenteVendasDTO>> GetByIdAsync(Guid id)
    {
        var gerente = await _repository.GetByIdAsync(id);
        if (gerente is null)
            return Result<GerenteVendasDTO>.Fail("Gerente de vendas não encontrado");

        return Result<GerenteVendasDTO>.Ok(GerenteVendasMapping.ToDto(gerente));
    }

    public async Task<Result<IEnumerable<GerenteVendasListaDTO>>> GetAllAsync()
    {
        var gerentes = await _repository.GetAllAsync();
        return Result<IEnumerable<GerenteVendasListaDTO>>.Ok(
            gerentes.Select(GerenteVendasMapping.ToListaDto));
    }

    public async Task<Result<Guid>> AddAsync(CriarGerenteVendasDTO dto)
    {
        try
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            var gerente = GerenteVendasMapping.ToEntity(dto, senhaHash);

            await _repository.AddAsync(gerente);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(gerente.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar gerente de vendas: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(AtualizarGerenteVendasDTO dto)
    {
        var gerente = await _repository.GetByIdAsync(dto.Id);
        if (gerente is null)
            return Result.Fail("Gerente de vendas não encontrado");

        try
        {
            GerenteVendasMapping.UpdateEntity(gerente, dto);
            _repository.Update(gerente);
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
        var gerente = await _repository.GetByIdAsync(id);
        if (gerente is null)
            return Result.Fail("Gerente de vendas não encontrado");

        _repository.Remove(gerente);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }
}
