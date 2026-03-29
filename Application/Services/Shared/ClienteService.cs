using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Shared;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<ClienteDTO>> GetByIdAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);

        if (cliente is null)
            return Result<ClienteDTO>.Fail("Cliente não encontrado");

        return Result<ClienteDTO>.Ok(
            ClienteMapping.ToDto(cliente)
        );
    }

    public async Task<Result<IEnumerable<ClienteListaDTO>>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();

        var lista = clientes
            .Select(ClienteMapping.ToListaDto);

        return Result<IEnumerable<ClienteListaDTO>>.Ok(lista);
    }

    public async Task<Result<ClienteDTO>> ObterPorCpfAsync(string cpf)
    {
        var cliente = await _repository.ObterPorCpfAsync(cpf);

        if (cliente is null)
            return Result<ClienteDTO>.Fail("Cliente não encontrado");

        return Result<ClienteDTO>.Ok(ClienteMapping.ToDto(cliente));
    }
    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result<Guid>> AddAsync(CriarClienteDTO dto)
    {
        if (await _repository.CpfExisteAsync(dto.Cpf))
            return Result<Guid>.Fail("CPF já cadastrado");

        try
        {
            var cliente = new Cliente(
                dto.Nome,
                new Cpf(dto.Cpf),
                new Telefone(dto.Telefone),
                new Email(dto.Email)
            );

            await _repository.AddAsync(cliente);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(cliente.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar cliente: {ex.Message}");
        }
    }

    // =========================
    // ATUALIZAÇÃO
    // =========================

    public async Task<Result> UpdateAsync(AtualizarClienteDTO dto)
    {
        var cliente = await _repository.GetByIdAsync(dto.Id);

        if (cliente is null)
            return Result.Fail("Cliente não encontrado");

        try
        {
            cliente.AtualizarDados(
                new Telefone(dto.Telefone),
                new Email (dto.Email)
            );

            _repository.Update(cliente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    // =========================
    // REMOÇÃO
    // =========================

    public async Task<Result> RemoveAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);

        if (cliente is null)
            return Result.Fail("Cliente não encontrado");

        _repository.Remove(cliente);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}