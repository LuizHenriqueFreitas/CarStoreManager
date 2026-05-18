using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class DespesaService : IDespesaService
{
    private readonly IDespesaRepository _repo;

    public DespesaService(IDespesaRepository repo) => _repo = repo;

    public async Task<Result<IEnumerable<DespesaDTO>>> GetAllAsync()
    {
        var lista = await _repo.GetAllAsync();
        return Result<IEnumerable<DespesaDTO>>.Ok(lista.Select(MapToDto));
    }

    public async Task<Result<DespesaDTO>> GetByIdAsync(Guid id)
    {
        var d = await _repo.GetByIdAsync(id);
        return d is null
            ? Result<DespesaDTO>.Fail("Despesa não encontrada.")
            : Result<DespesaDTO>.Ok(MapToDto(d));
    }

    public async Task<Result<Guid>> AddAsync(CriarDespesaDTO dto)
    {
        try
        {
            var setor = ParseSetor(dto.Setor);
            var despesa = new Despesa(dto.Nome, dto.Valor, setor);
            await _repo.AddAsync(despesa);
            await _repo.SaveChangesAsync();
            return Result<Guid>.Ok(despesa.Id);
        }
        catch (ArgumentException ex) { return Result<Guid>.Fail(ex.Message); }
    }

    public async Task<Result> UpdateAsync(AtualizarDespesaDTO dto)
    {
        var despesa = await _repo.GetByIdAsync(dto.Id);
        if (despesa is null) return Result.Fail("Despesa não encontrada.");

        try
        {
            despesa.Atualizar(dto.Nome, dto.Valor);
            despesa.AtualizarSetor(ParseSetor(dto.Setor));
            if (dto.Ativa && !despesa.Ativa) despesa.Reativar();
            else if (!dto.Ativa && despesa.Ativa) despesa.Desativar();

            _repo.Update(despesa);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }
    }

    private static SetorDespesa ParseSetor(string? setor)
    {
        if (string.IsNullOrWhiteSpace(setor)) return SetorDespesa.Geral;
        return Enum.TryParse<SetorDespesa>(setor, true, out var s) ? s : SetorDespesa.Geral;
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var despesa = await _repo.GetByIdAsync(id);
        if (despesa is null) return Result.Fail("Despesa não encontrada.");

        _repo.Remove(despesa);
        await _repo.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<decimal>> ObterTotalMensalAsync()
    {
        var ativas = await _repo.GetAtivasAsync();
        return Result<decimal>.Ok(ativas.Sum(d => d.GetValor()));
    }

    private static DespesaDTO MapToDto(Despesa d) => new()
    {
        Id = d.Id,
        Nome = d.Nome,
        Valor = d.GetValor(),
        Ativa = d.Ativa,
        Setor = d.Setor.ToString(),
        DataUltimaAtualizacao = d.DataUltimaAtualizacao
    };
}
