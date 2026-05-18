using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.ChecklistPreset;
using CarStoreManager.Application.Interfaces.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;

namespace CarStoreManager.Application.Services.Oficina;

public class ChecklistPresetService : IChecklistPresetService
{
    private readonly IChecklistPresetRepository _repo;

    public ChecklistPresetService(IChecklistPresetRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<ChecklistPresetDTO>>> GetAllAsync()
    {
        var lista = await _repo.GetAllAsync();
        return Result<IEnumerable<ChecklistPresetDTO>>.Ok(lista.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<ChecklistPresetLookupDTO>>> GetLookupAtivosAsync()
    {
        var lista = await _repo.GetAtivosAsync();
        return Result<IEnumerable<ChecklistPresetLookupDTO>>.Ok(
            lista.Select(p => new ChecklistPresetLookupDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                QuantidadeItens = p.Itens.Count
            }));
    }

    public async Task<Result<ChecklistPresetDTO>> GetByIdAsync(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p is null
            ? Result<ChecklistPresetDTO>.Fail("Preset não encontrado.")
            : Result<ChecklistPresetDTO>.Ok(MapToDto(p));
    }

    public async Task<Result<Guid>> AddAsync(SalvarChecklistPresetDTO dto)
    {
        try
        {
            var preset = new ChecklistPreset(dto.Nome);
            preset.SubstituirItens(dto.Itens ?? new());
            if (!dto.Ativo) preset.Desativar();

            await _repo.AddAsync(preset);
            await _repo.SaveChangesAsync();
            return Result<Guid>.Ok(preset.Id);
        }
        catch (ArgumentException ex) { return Result<Guid>.Fail(ex.Message); }
    }

    public async Task<Result> UpdateAsync(SalvarChecklistPresetDTO dto)
    {
        var preset = await _repo.GetByIdAsync(dto.Id);
        if (preset is null) return Result.Fail("Preset não encontrado.");

        try
        {
            preset.AtualizarNome(dto.Nome);
            preset.SubstituirItens(dto.Itens ?? new());
            if (dto.Ativo && !preset.Ativo) preset.Reativar();
            else if (!dto.Ativo && preset.Ativo) preset.Desativar();

            _repo.Update(preset);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var preset = await _repo.GetByIdAsync(id);
        if (preset is null) return Result.Fail("Preset não encontrado.");

        _repo.Remove(preset);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    private static ChecklistPresetDTO MapToDto(ChecklistPreset p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Ativo = p.Ativo,
        DataUltimaAtualizacao = p.DataUltimaAtualizacao,
        Itens = p.Itens
            .OrderBy(i => i.Ordem)
            .Select(i => new ChecklistPresetItemDTO
            {
                Id = i.Id,
                Descricao = i.Descricao,
                Ordem = i.Ordem
            }).ToList()
    };
}
