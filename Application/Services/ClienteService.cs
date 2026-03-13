using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<ClienteDTO>> ObterPorIdAsync(Guid id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id);

        if (cliente == null)
            return Result<ClienteDTO>.Fail("Cliente não encontrado");

        var dto = new ClienteDTO(
            cliente.Id,
            cliente.Nome,
            cliente.Documento,
            cliente.Telefone,
            cliente.Email
        );

        return Result<ClienteDTO>.Ok(dto);
    }

    public async Task<Result<IEnumerable<ClienteDTO>>> ObterTodosAsync()
    {
        var clientes = await _clienteRepository.ObterTodosAsync();

        var lista = clientes.Select(c =>
            new ClienteDTO(
                c.Id,
                c.Nome,
                c.Documento,
                c.Telefone,
                c.Email
            )
        );

        return Result<IEnumerable<ClienteDTO>>.Ok(lista);
    }

    public async Task<Result> CriarAsync(ClienteDTO clienteDto)
    {
        var cliente = new Cliente(
            clienteDto.Nome,
            clienteDto.Documento,
            clienteDto.Telefone,
            clienteDto.Email
        );

        await _clienteRepository.AdicionarAsync(cliente);

        return Result.Ok();
    }

    public async Task<Result> AtualizarAsync(ClienteDTO clienteDto)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteDto.Id);

        if (cliente == null)
            return Result.Fail("Cliente não encontrado");

        cliente.AtualizarDados(
            clienteDto.Nome,
            clienteDto.Telefone,
            clienteDto.Email
        );

        await _clienteRepository.AtualizarAsync(cliente);

        return Result.Ok();
    }

    public async Task<Result> RemoverAsync(Guid id)
    {
        await _clienteRepository.RemoverAsync(id);
        return Result.Ok();
    }
}