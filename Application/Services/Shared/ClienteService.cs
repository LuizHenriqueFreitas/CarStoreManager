using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Shared;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de ClientService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    /*
        metodo de busca por id valida que
        caso cliente buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<ClienteDTO>> GetByIdAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);

        if (cliente is null)
            return Result<ClienteDTO>.Fail("Cliente não encontrado");

        return Result<ClienteDTO>.Ok(
            ClienteMapping.ToDto(cliente)
        );
    }

    //busca todos os clientes
    public async Task<Result<IEnumerable<ClienteListaDTO>>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();

        var lista = clientes
            .Select(ClienteMapping.ToListaDto);

        return Result<IEnumerable<ClienteListaDTO>>.Ok(lista);
    }

    /*
        metodo de busca por CPF valida que
        caso cliente buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<ClienteDTO>> ObterPorCpfAsync(string cpf)
    {
        var cliente = await _repository.ObterPorCpfAsync(cpf);

        if (cliente is null)
            return Result<ClienteDTO>.Fail("Cliente não encontrado");

        return Result<ClienteDTO>.Ok(ClienteMapping.ToDto(cliente));
    }

    /*
        metodo de busca por termo faz uma
        lista com todos os clientes que tenha
        o termo em questao em algum dos campos
        de seu cadastro, nome, email, CPF, etc
    */
    public async Task<Result<List<ClienteListaDTO>>> PesquisarAsync(string termo)
    {
        try
        {
            var clientes = await _repository.PesquisarAsync(termo);

            var dtos = clientes.Select(c => new ClienteListaDTO
            {
                Id = c.Id,
                Nome = c.Nome,
                Cpf = c.GetCpf(), 
                Telefone = c.GetTelefone(),
                Email = c.GetEmail()
            }).ToList();

            return Result<List<ClienteListaDTO>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<ClienteListaDTO>>.Fail($"Erro na pesquisa: {ex.Message}");
        }
    }
    
    /*
        metodo para criar novo cliente
        bloqueia criar outro cliente
        com CPF ja cadastrado no sistema
    */
    public async Task<Result<Guid>> AddAsync(CriarClienteDTO dto)
    {
        if (await _repository.CpfExisteAsync(dto.Cpf))
            return Result<Guid>.Fail("CPF já cadastrado");

        try
        {
            var cliente = new Cliente(
                dto.Nome,
                dto.Email,
                dto.Telefone,
                dto.Cpf
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

    /*
        metodo que atualiza cliente ja existente
        faz busca por id e caso cliente seja vazio 
        ele retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(AtualizarClienteDTO dto)
    {
        var cliente = await _repository.GetByIdAsync(dto.Id);

        if (cliente is null)
            return Result.Fail("Cliente não encontrado");

        try
        {
            cliente.AtualizarClienteDados(
                dto.Nome,
                dto.Telefone,
                dto.Email
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

    /*
        metodo que remove cliente por id
        caso seja vazio retona 
        o aviso que nao foi encontrado
    */
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