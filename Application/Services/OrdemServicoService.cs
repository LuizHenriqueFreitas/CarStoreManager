using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Repositories;
using Domain.Interfaces;
using SistemaEmpresa.Domain.Entities.Oficina;

namespace CarStoreManager.Application.Services;

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _repository;

    public OrdemServicoService(IOrdemServicoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> CriarAsync(OrdemServicoDTO ordemDto)
    {
        var ordem = new OrdemServico(
            ordemDto.MecanicoId,
            ordemDto.Descricao,
            ordemDto.PrazoEstimado,
            ordemDto.ValorTotal
        );

        await _repository.AdicionarAsync(ordem);

        return Result.Ok();
    }

    public async Task<Result<OrdemServicoDTO>> ObterPorIdAsync(Guid id)
    {
        var ordem = await _repository.ObterPorIdAsync(id);

        if (ordem == null)
            return Result<OrdemServicoDTO>.Fail("Ordem de serviço não encontrada");

        var dto = new OrdemServicoDTO(
            ordem.Id,
            ordem.ClienteId,
            ordem.MecanicoId,
            ordem.VeiculoId,
            ordem.Descricao,
            ordem.DataCriacao,
            ordem.PrazoEstimado,
            ordem.CustoPecas,
            ordem.CustoServico,
            ordem.ValorTotal,
            ordem.Status
        );

        return Result<OrdemServicoDTO>.Ok(dto);
    }

    public async Task<Result<IEnumerable<OrdemServicoDTO>>> ObterTodasAsync()
    {
        var ordens = await _repository.ObterTodasAsync();

        var lista = ordens.Select(o =>
            new OrdemServicoDTO(
                o.Id,
                o.ClienteId,
                o.MecanicoId,
                o.VeiculoId,
                o.Descricao,
                o.DataCriacao,
                o.PrazoEstimado,
                o.CustoPecas,
                o.CustoServico,
                o.ValorTotal,
                o.Status
            )
        );

        return Result<IEnumerable<OrdemServicoDTO>>.Ok(lista);
    }

    public async Task<Result> AtualizarStatusAsync(Guid id, string status)
    {
        var ordem = await _repository.ObterPorIdAsync(id);

        if (ordem == null)
            return Result.Fail("Ordem não encontrada");

        ordem.AtualizarStatus(status);

        await _repository.AtualizarAsync(ordem);

        return Result.Ok();
    }
}