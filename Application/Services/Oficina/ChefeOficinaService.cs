using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.ChefeOficina;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;

namespace CarStoreManager.Application.Services.Oficina;

public class ChefeOficinaService : IChefeOficinaService
{
    private readonly IChefeOficinaRepository _repository;

    public ChefeOficinaService(IChefeOficinaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ChefeOficinaDTO>> GetByIdAsync(Guid id)
    {
        var chefe = await _repository.GetByIdAsync(id);
        if (chefe is null)
            return Result<ChefeOficinaDTO>.Fail("Chefe de oficina não encontrado");

        return Result<ChefeOficinaDTO>.Ok(ChefeOficinaMapping.ToDto(chefe));
    }

    public async Task<Result<IEnumerable<ChefeOficinaListaDTO>>> GetAllAsync()
    {
        var chefes = await _repository.GetAllAsync();
        return Result<IEnumerable<ChefeOficinaListaDTO>>.Ok(
            chefes.Select(ChefeOficinaMapping.ToListaDto));
    }

    public async Task<Result<Guid>> AddAsync(CriarChefeOficinaDTO dto)
    {
        try
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            var chefe = ChefeOficinaMapping.ToEntity(dto, senhaHash);

            await _repository.AddAsync(chefe);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(chefe.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar chefe de oficina: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(AtualizarChefeOficinaDTO dto)
    {
        var chefe = await _repository.GetByIdAsync(dto.Id);
        if (chefe is null)
            return Result.Fail("Chefe de oficina não encontrado");

        try
        {
            ChefeOficinaMapping.UpdateEntity(chefe, dto);
            _repository.Update(chefe);
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
        var chefe = await _repository.GetByIdAsync(id);
        if (chefe is null)
            return Result.Fail("Chefe de oficina não encontrado");

        _repository.Remove(chefe);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }
}
