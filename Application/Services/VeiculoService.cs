using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using SistemaEmpresa.Domain.Entities;
using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace CarStoreManager.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repository;

    public VeiculoService(IVeiculoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<VeiculoDTO>>> ObterTodosAsync()
    {
        var veiculo = await _repository.ObterTodosAsync();

        var lista = veiculo.Select(v =>
            new VeiculoDTO(
                v.Id,
                v.Marca,
                v.Modelo,
                v.Ano,
                v.Cor,
                v.Quilometragem,
                v.Estado,
                v.Placa,
                v.Valor,
                v.Disponivel
            ));
        
        return Result<IEnumerable<VeiculoDTO>>.Ok(lista);
    }

    public async Task<Result<VeiculoDTO>> ObterPorIdAsync(Guid id)
    {
        var veiculo = await _repository.ObterPorIdAsync(id);

        if (veiculo == null)
            return Result<VeiculoDTO>.Fail("Veículo não encontrado");

        var dto = new VeiculoDTO(
            veiculo.Id,
            veiculo.Marca,
            veiculo.Modelo,
            veiculo.Ano,
            veiculo.Cor,
            veiculo.Quilometragem,
            veiculo.Estado,
            veiculo.Placa,
            veiculo.Valor,
            veiculo.Disponivel
        );

        return Result<VeiculoDTO>.Ok(dto);
    }

    public async Task<Result<IEnumerable<VeiculoDTO>>> ObterPorClienteAsync(Guid clienteId)
    {
        var veiculos = await _repository.ObterPorClienteAsync(clienteId);

        var lista = veiculos.Select(v =>
            new VeiculoDTO(
                v.Id,
                v.Marca, 
                v.Modelo, 
                v.Ano,
                v.Cor,
                v.Quilometragem,
                v.Estado,
                v.Placa,
                v.Valor,
                v.Disponivel 
            ));

        return Result<IEnumerable<VeiculoDTO>>.Ok(lista);
    }

    public async Task<Result> CriarAsync(VeiculoDTO dto)
    {
        var veiculo = new Veiculo(
            dto.Id,
            dto.Marca, 
            dto.Modelo, 
            dto.Ano,
            dto.Cor,
            dto.Quilometragem,
            dto.Estado,
            dto.Placa,
            dto.Valor,
            dto.Disponivel 
        );

        await _repository.AdicionarAsync(veiculo);

        return Result.Ok();
    }

    public async Task<Result> AtualizarAsync(VeiculoDTO dto)
    {
        var veiculo = await _repository.ObterPorIdAsync(dto.Id);

        if (veiculo == null)
            return Result.Fail("Veículo não encontrado");

        veiculo.AtualizarDados(
            dto.Marca, 
            dto.Modelo, 
            dto.Ano, 
            dto.Cor, 
            dto.Quilometragem, 
            dto.Estado, 
            dto.Placa, 
            dto.Valor, 
            dto.Disponivel);

        await _repository.AtualizarAsync(veiculo);

        return Result.Ok();
    }

    public async Task<Result> RemoverAsync(Guid id)
    {
        await _repository.RemoverAsync(id);
        return Result.Ok();
    }
}