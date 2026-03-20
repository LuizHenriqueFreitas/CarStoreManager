using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class PropostaVendaService : IPropostaVendaService
{
    private readonly IPropostaVendaRepository _repository;

    public PropostaVendaService(IPropostaVendaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> CriarAsync(PropostaVendaDTO dto)
    {
        var proposta = new PropostaVenda(
            dto.VendedorId,
            dto.VeiculoId,
            dto.ClienteId,
            dto.ValorBase,
            dto.Desconto,
            dto.ValorFinal,
            dto.Entrada,
            dto.ValorFinanciado,
            dto.Parcelas,
            dto.DataCriacao,
            dto.Status
        );

        await _repository.AdicionarAsync(proposta);

        return Result.Ok();
    }

    public async Task<Result<PropostaVendaDTO>> ObterPorIdAsync(Guid id)
    {
        var proposta = await _repository.ObterPorIdAsync(id);

        if (proposta == null)
            return Result<PropostaVendaDTO>.Fail("Proposta não encontrada");

        var dto = new PropostaVendaDTO(
            proposta.Id,
            proposta.VendedorId,
            proposta.VeiculoId,
            proposta.ClienteId,
            proposta.ValorBase,
            proposta.Desconto,
            proposta.ValorFinal,
            proposta.Entrada,
            proposta.ValorFinanciado,
            proposta.Parcelas,
            proposta.DataCriacao,
            proposta.Status
        );

        return Result<PropostaVendaDTO>.Ok(dto);
    }

    public async Task<Result<IEnumerable<PropostaVendaDTO>>> ObterTodasAsync()
    {
        var propostas = await _repository.ObterTodasAsync();

        var lista = propostas.Select(p =>
            new PropostaVendaDTO(
                p.Id,
                p.VendedorId,
                p.VeiculoId,
                p.ClienteId,
                p.ValorBase,
                p.Desconto,
                p.ValorFinal,
                p.Entrada,
                p.ValorFinanciado,
                p.Parcelas,
                p.DataCriacao,
                p.Status
            )
        );

        return Result<IEnumerable<PropostaVendaDTO>>.Ok(lista);
    }

    public async Task<Result> AtualizarStatusAsync(Guid id, string status)
    {
        var proposta = await _repository.ObterPorIdAsync(id);

        if (proposta == null)
            return Result.Fail("Proposta não encontrada");

        proposta.AtualizarStatus(status);

        await _repository.AtualizarAsync(proposta);

        return Result.Ok();
    }
}