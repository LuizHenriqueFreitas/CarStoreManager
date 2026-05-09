using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de MecanicoService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class MecanicoService : IMecanicoService
{
    private readonly IMecanicoRepository _repository;

    public MecanicoService(IMecanicoRepository repository)
    {
        _repository = repository;
    }

    /*
        metodo de busca por id valida que
        caso mecanico buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<MecanicoDTO>> GetByIdAsync(Guid id)
    {
        var mecanico = await _repository.GetByIdAsync(id);

        if (mecanico is null)
            return Result<MecanicoDTO>.Fail("Mecânico não encontrado");

        return Result<MecanicoDTO>.Ok(MecanicoMapping.ToDto(mecanico));
    }

    //busca todos os mecanicos
    public async Task<Result<IEnumerable<MecanicoListaDTO>>> GetAllAsync()
    {
        var lista = (await _repository.GetAllAsync())
            .Select(MecanicoMapping.ToListaDto);

        return Result<IEnumerable<MecanicoListaDTO>>.Ok(lista);
    }

    //metodo para criar novo mecanico
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

    /*
        metodo que atualiza mecanico ja existente
        faz busca por id e caso mecanico seja vazio 
        ele retona o aviso que nao foi encontrado
    */
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

    //metodo que filtra apenas mecanicos com status de disponivel
    public async Task<Result<IEnumerable<MecanicoLookupDTO>>> ObterDisponiveisAsync()
    {
        var mecanicos = await _repository.GetAllAsync();

        var lista = mecanicos
            .Select(MecanicoMapping.ToLookupDto);

        return Result<IEnumerable<MecanicoLookupDTO>>.Ok(lista);
    }

    /*
        metodo que atualiza o nivel de ocupação dos mecanicos
        caso o mecanico buscado por id seja vazio
        ele retorna o aviso que o mecanico nao foi encontrado
    */
    public async Task<Result> AtualizarOcupacaoAsync(Guid mecanicoId)
    {
        var mecanico = await _repository.GetByIdAsync(mecanicoId);
        if (mecanico is null)
            return Result.Fail("Mecânico não encontrado");

        mecanico.AlterarOcupado();
        _repository.Update(mecanico);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    /*
        metodo que remove mecanico por id
        caso seja vazio retona 
        o aviso que nao foi encontrado
    */
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