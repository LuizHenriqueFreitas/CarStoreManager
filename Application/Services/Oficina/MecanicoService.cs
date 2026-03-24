using System.Diagnostics;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class MecanicoService : IMecanicoService
{
    private readonly IMecanicoRepository _repository;

    public MecanicoService(IMecanicoRepository repository)
    {
        _repository = repository;
    }

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<MecanicoDTO>> ObterPorIdAsync(Guid id)
    {
        var mecanico = await _repository.GetByIdAsync(id);

        if (mecanico is null)
            return Result<MecanicoDTO>.Fail("Mecânico não encontrado");

        return Result<MecanicoDTO>.Ok(
            MecanicoMapping.ToDto(mecanico)
        );
    }

    public async Task<Result<IEnumerable<MecanicoListaDTO>>> ObterTodosAsync()
    {
        var mecanicos = await _repository.GetAllAsync();

        var lista = mecanicos
            .Select(MecanicoMapping.ToListaDto);

        return Result<IEnumerable<MecanicoListaDTO>>.Ok(lista);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result<Guid>> CriarAsync(CriarMecanicoDTO dto)
    {
        if(!Enum.TryParse<EspecialidadeMecanico>(dto.Especialidade, true, out var especialidade))
            return Result<Guid>.Fail("Especialidade Inválida");

        try
        {
            var mecanico = new Mecanico(
                dto.Nome,
                new Email(dto.Email),
                new Telefone(dto.Telefone),
                especialidade,
                new Dinheiro(dto.ValorHora)
            );

            mecanico.DefinirContato(
                new Email(dto.Email),
                new Telefone(dto.Telefone)
            );

            await _repository.AddAsync(mecanico);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(mecanico.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar mecânico: {ex.Message}");
        }
    }

    // =========================
    // ATUALIZAÇÃO
    // =========================

    public async Task<Result> AtualizarAsync(AtualizarMecanicoDTO dto)
    {
        var mecanico = await _repository.GetByIdAsync(dto.Id);

        if (mecanico is null)
            return Result.Fail("Mecânico não encontrado");

        if(!Enum.TryParse<EspecialidadeMecanico>(dto.Especialidade, true, out var especialidade))
            return Result.Fail("Especialidade Inválida");

        try
        {
            mecanico.AtualizarDados(
                dto.Nome,
                new Email(dto.Email),
                new Telefone(dto.Telefone),
                especialidade,
                new Dinheiro(dto.ValorHora)
            );

            mecanico.DefinirContato(
                new Email(dto.Email),
                new Telefone(dto.Telefone)
            );

            _repository.Update(mecanico);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // OCUPAÇÃO (REGRA DE NEGÓCIO)
    // =========================

    // revisar essa parte - na interface tambem
    public async Task<Result> AtualizarStatusOcupacaoAsync(Guid id)
    {
        var mecanico = await _repository.GetByIdAsync(id);

        if (mecanico is null)
            return Result.Fail("Mecânico não encontrado");

        mecanico.AtribuirOrdem();

        _repository.Update(mecanico);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> AtualizarOcupacaoAsync(Guid mecanicoId)
{
    var mecanico = await _repository.GetByIdAsync(mecanicoId);

    if (mecanico is null)
        return Result.Fail("Mecânico não encontrado");

    try
    {
        //trocar isso depois
        mecanico.AtribuirOrdem();

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
            .Where(m => m.NivelOcupacao != NivelOcupacaoMecanico.Ocupado)
            .Select(MecanicoMapping.ToLookupDto);

        return Result<IEnumerable<MecanicoLookupDTO>>.Ok(lista);
    }
    // =========================
    // REMOÇÃO
    // =========================

    public async Task<Result> RemoverAsync(Guid id)
    {
        var mecanico = await _repository.GetByIdAsync(id);

        if (mecanico is null)
            return Result.Fail("Mecânico não encontrado");

        _repository.Remove(mecanico);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}