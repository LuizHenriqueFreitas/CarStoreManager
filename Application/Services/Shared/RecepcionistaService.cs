using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Recepcionista;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Shared;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Shared;

public class RecepcionistaService : IRecepcionistaService
{
    private readonly IRecepcionistaRepository _repository;

    public RecepcionistaService(IRecepcionistaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<RecepcionistaDTO>> GetByIdAsync(Guid id)
    {
        var rec = await _repository.GetByIdAsync(id);
        if (rec is null)
            return Result<RecepcionistaDTO>.Fail("Recepcionista não encontrado");

        return Result<RecepcionistaDTO>.Ok(RecepcionistaMapping.ToDto(rec));
    }

    public async Task<Result<IEnumerable<RecepcionistaListaDTO>>> GetAllAsync()
    {
        var recs = await _repository.GetAllAsync();
        return Result<IEnumerable<RecepcionistaListaDTO>>.Ok(
            recs.Select(RecepcionistaMapping.ToListaDto));
    }

    public async Task<Result<Guid>> AddAsync(CriarRecepcionistaDTO dto)
    {
        try
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            var rec = RecepcionistaMapping.ToEntity(dto, senhaHash);

            await _repository.AddAsync(rec);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(rec.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar recepcionista: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(AtualizarRecepcionistaDTO dto)
    {
        var rec = await _repository.GetByIdAsync(dto.Id);
        if (rec is null)
            return Result.Fail("Recepcionista não encontrado");

        try
        {
            RecepcionistaMapping.UpdateEntity(rec, dto);
            _repository.Update(rec);
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
        var rec = await _repository.GetByIdAsync(id);
        if (rec is null)
            return Result.Fail("Recepcionista não encontrado");

        _repository.Remove(rec);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }
}
