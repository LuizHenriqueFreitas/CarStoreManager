using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class PropostaVendaService : IPropostaVendaService
{
    private readonly IPropostaVendaRepository _repository;

    public PropostaVendaService(IPropostaVendaRepository repository)
    {
        _repository = repository;
    }

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<PropostaVendaDTO>> ObterPorIdAsync(Guid id)
    {
        var proposta = await _repository.GetByIdAsync(id);

        if (proposta is null)
            return Result<PropostaVendaDTO>.Fail("Proposta não encontrada");

        return Result<PropostaVendaDTO>.Ok(
            PropostaVendaMapping.ToDto(proposta)
        );
    }

    public async Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterTodasAsync()
    {
        var propostas = await _repository.GetAllAsync();

        var lista = propostas
            .Select(PropostaVendaMapping.ToListaDto);

        return Result<IEnumerable<PropostaVendaListaDTO>>.Ok(lista);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result<Guid>> CriarAsync(CriarPropostaVendaDTO dto)
    {
        try
        {
            var proposta = new PropostaVenda(
                dto.VendedorId,
                dto.VeiculoId,
                dto.ClienteId,
                new Dinheiro(dto.ValorBase)
            );

            await _repository.AddAsync(proposta);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(proposta.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar proposta: {ex.Message}");
        }
    }

    // =========================
    // REGRAS DE NEGÓCIO
    // =========================

    public async Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);

        if (proposta is null)
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.AplicarDesconto(new Percentual(dto.Percentual));

            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);

        if (proposta is null)
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.DefinirEntrada(new Dinheiro(dto.ValorEntrada));

            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto)
    {
        var proposta = await _repository.GetByIdAsync(dto.PropostaId);

        if (proposta is null)
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.GerarFinanciamento(new Parcelas(dto.Parcelas));

            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // STATUS / FLUXO
    // =========================

    public async Task<Result> AprovarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);

        if (proposta is null)
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.Aprovar();

            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> RejeitarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);

        if (proposta is null)
            return Result.Fail("Proposta não encontrada");

        try
        {
            proposta.Rejeitar();

            _repository.Update(proposta);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> CancelarAsync(Guid propostaId)
    {
        var proposta = await _repository.GetByIdAsync(propostaId);

        if (proposta is null)
            return Result.Fail("Proposta não encontrada");

        proposta.Cancelar();

        _repository.Update(proposta);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}