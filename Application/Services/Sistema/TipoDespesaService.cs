using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class TipoDespesaService : ITipoDespesaService
{
    private readonly ITipoDespesaRepository _repo;
    private readonly IDespesaRepository _despesaRepo;

    public TipoDespesaService(ITipoDespesaRepository repo, IDespesaRepository despesaRepo)
    {
        _repo = repo;
        _despesaRepo = despesaRepo;
    }

    public async Task<Result<IEnumerable<TipoDespesaDTO>>> GetAllAsync()
    {
        var lista = await _repo.GetAllAsync();
        return Result<IEnumerable<TipoDespesaDTO>>.Ok(lista.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<TipoDespesaDTO>>> GetAtivosAsync()
    {
        var lista = await _repo.GetAtivosAsync();
        return Result<IEnumerable<TipoDespesaDTO>>.Ok(lista.Select(MapToDto));
    }

    public async Task<Result<Guid>> AddAsync(SalvarTipoDespesaDTO dto)
    {
        try
        {
            var tipo = new TipoDespesa(dto.Nome);
            if (!dto.Ativo) tipo.Desativar();

            await _repo.AddAsync(tipo);
            await _repo.SaveChangesAsync();
            return Result<Guid>.Ok(tipo.Id);
        }
        catch (ArgumentException ex) { return Result<Guid>.Fail(ex.Message); }
    }

    public async Task<Result> UpdateAsync(SalvarTipoDespesaDTO dto)
    {
        var tipo = await _repo.GetByIdAsync(dto.Id);
        if (tipo is null) return Result.Fail("Tipo de despesa não encontrado.");

        try
        {
            tipo.Renomear(dto.Nome);
            if (dto.Ativo && !tipo.Ativo) tipo.Reativar();
            else if (!dto.Ativo && tipo.Ativo) tipo.Desativar();

            _repo.Update(tipo);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var tipo = await _repo.GetByIdAsync(id);
        if (tipo is null) return Result.Fail("Tipo de despesa não encontrado.");

        // Não excluir um tipo em uso — desative-o (mantém o histórico das despesas).
        var emUso = (await _despesaRepo.GetPorTipoAsync(id)).Any();
        if (emUso)
            return Result.Fail("Tipo em uso por despesas — desative-o em vez de excluir.");

        _repo.Remove(tipo);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    private static TipoDespesaDTO MapToDto(TipoDespesa t) => new()
    {
        Id = t.Id,
        Nome = t.Nome,
        Ativo = t.Ativo,
        DataUltimaAtualizacao = t.DataUltimaAtualizacao
    };
}
