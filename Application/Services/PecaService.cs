using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class PecaService : IPecaService
{
    private readonly IPecaRepository _repository;

    public PecaService(IPecaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<PecaDTO>>> ObterTodosAsync()
    {
        var pecas = await _repository.ObterTodosAsync();

        var lista = pecas.Select(p =>
            new PecaDTO(p.Id, p.Nome, p.Modelo, p.Valor, p.QuantidadeEstoque)
        );

        return Result<IEnumerable<PecaDTO>>.Ok(lista);
    }

    public async Task<Result<PecaDTO>> ObterPorIdAsync(Guid id)
    {
        var peca = await _repository.ObterPorIdAsync(id);

        if (peca == null)
            return Result<PecaDTO>.Fail("Peça não encontrada");

        var dto = new PecaDTO(
            peca.Id,
            peca.Nome,
            peca.Modelo,
            peca.Valor,
            peca.QuantidadeEstoque
        );

        return Result<PecaDTO>.Ok(dto);
    }

    public async Task<Result> CriarAsync(PecaDTO dto)
    {
        var peca = new Peca(
            dto.Nome,
            dto.Modelo,
            dto.Valor,
            dto.QuantidadeEstoque
        );

        await _repository.AdicionarAsync(peca);

        return Result.Ok();
    }

    public async Task<Result> AtualizarAsync(PecaDTO PecaDto)
    {
        var peca = await _repository.ObterPorIdAsync(PecaDto.Id);

        if (peca == null)
            return Result.Fail("Peça não encontrada");

        peca.AtualizarDados(
            PecaDto.Nome,
            PecaDto.Modelo,
            PecaDto.Valor,
            PecaDto.QuantidadeEstoque
        );

        await _repository.AtualizarAsync(peca);

        return Result.Ok();
    }

    public async Task<Result> RemoverAsync(Guid id)
    {
        await _repository.RemoverAsync(id);
        return Result.Ok();
    }
}