using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class MecanicoService : IMecanicoService
{
    private readonly IMecanicoRepository _repository;

    public MecanicoService(IMecanicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<MecanicoDTO>> GetByIdAsync(Guid id)
    {
        var mecanico = await _repository.GetByIdAsync(id);

        if (mecanico is null)
            return Result<MecanicoDTO>.Fail("Mecânico não encontrado");

        return Result<MecanicoDTO>.Ok(MecanicoMapping.ToDto(mecanico));
    }

    public async Task<Result<IEnumerable<MecanicoListaDTO>>> GetAllAsync()
    {
        var lista = (await _repository.GetAllAsync())
            .Select(MecanicoMapping.ToListaDto);

        return Result<IEnumerable<MecanicoListaDTO>>.Ok(lista);
    }

    public async Task<Result<Guid>> AddAsync(CriarMecanicoDTO dto)
    {
        try
        {
            var mecanico = MecanicoMapping.ToEntity(dto);

            await _repository.AddAsync(mecanico);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(mecanico.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail(ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(AtualizarMecanicoDTO dto)
    {
        var mecanico = await _repository.GetByIdAsync(dto.Id);

        if (mecanico is null)
            return Result.Fail("Mecânico não encontrado");

        try
        {
            MecanicoMapping.UpdateEntity(mecanico, dto);

            _repository.Update(mecanico);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<MecanicoLookupDTO>>> ObterDisponiveisAsync()
    {
        var mecanicos = await _repository.GetAllAsync();

        var lista = mecanicos
            .Select(MecanicoMapping.ToLookupDto);

        return Result<IEnumerable<MecanicoLookupDTO>>.Ok(lista);
    }

    public async Task<Result> AtualizarOcupacaoAsync(Guid mecanicoId)
    {
        // ⚠️ Aqui depende de OrdemServico → deixei preparado
        return Result.Ok();
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var mecanico = await _repository.GetByIdAsync(id);

        if (mecanico is null)
            return Result.Fail("Mecânico não encontrado");

        _repository.Remove(mecanico);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}