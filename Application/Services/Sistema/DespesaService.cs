using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class DespesaService : IDespesaService
{
    private readonly IDespesaRepository _repo;
    private readonly ITipoDespesaRepository _tipoRepo;

    public DespesaService(IDespesaRepository repo, ITipoDespesaRepository tipoRepo)
    {
        _repo = repo;
        _tipoRepo = tipoRepo;
    }

    public async Task<Result<IEnumerable<DespesaDTO>>> GetAllAsync()
    {
        var lista = await _repo.GetAllAsync();
        var tipos = (await _tipoRepo.GetAllAsync()).ToDictionary(t => t.Id, t => t.Nome);
        return Result<IEnumerable<DespesaDTO>>.Ok(lista.Select(d => MapToDto(d, tipos)));
    }

    public async Task<Result<DespesaDTO>> GetByIdAsync(Guid id)
    {
        var d = await _repo.GetByIdAsync(id);
        if (d is null) return Result<DespesaDTO>.Fail("Despesa não encontrada.");

        var tipo = await _tipoRepo.GetByIdAsync(d.TipoDespesaId);
        return Result<DespesaDTO>.Ok(MapToDto(d, tipo?.Nome ?? ""));
    }

    public async Task<Result<Guid>> AddAsync(CriarDespesaDTO dto)
    {
        var tipo = await _tipoRepo.GetByIdAsync(dto.TipoDespesaId);
        if (tipo is null) return Result<Guid>.Fail("Selecione um tipo de despesa válido.");

        try
        {
            var despesa = new Despesa(dto.Nome, dto.Valor, dto.TipoDespesaId);
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

        var tipo = await _tipoRepo.GetByIdAsync(dto.TipoDespesaId);
        if (tipo is null) return Result.Fail("Selecione um tipo de despesa válido.");

        try
        {
            despesa.Atualizar(dto.Nome, dto.Valor);
            despesa.AtualizarTipo(dto.TipoDespesaId);
            if (dto.Ativa && !despesa.Ativa) despesa.Reativar();
            else if (!dto.Ativa && despesa.Ativa) despesa.Desativar();

            _repo.Update(despesa);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }
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

    private static DespesaDTO MapToDto(Despesa d, IReadOnlyDictionary<Guid, string> tipos)
        => MapToDto(d, tipos.TryGetValue(d.TipoDespesaId, out var nome) ? nome : "");

    private static DespesaDTO MapToDto(Despesa d, string tipoNome) => new()
    {
        Id = d.Id,
        Nome = d.Nome,
        Valor = d.GetValor(),
        Ativa = d.Ativa,
        TipoDespesaId = d.TipoDespesaId,
        TipoNome = tipoNome,
        DataUltimaAtualizacao = d.DataUltimaAtualizacao
    };
}
