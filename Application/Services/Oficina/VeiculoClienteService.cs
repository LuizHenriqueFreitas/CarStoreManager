using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de VeiculoClienteService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class VeiculoClienteService : IVeiculoClienteService
{
    private readonly IVeiculoClienteRepository _repository;
    private readonly IClienteRepository _clienteRepository;

    public VeiculoClienteService(
        IVeiculoClienteRepository repository,
        IClienteRepository clienteRepository)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
    }

    /*
        metodo de busca por id valida que
        caso veiculoCliente buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<VeiculoClienteDTO>> GetByIdAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result<VeiculoClienteDTO>.Fail("Veículo não encontrado");

        return Result<VeiculoClienteDTO>.Ok(VeiculoClienteMapping.ToDto(veiculo));
    }

    //busca todos os veiculoCliente
    public async Task<Result<IEnumerable<VeiculoClienteListaDTO>>> GetAllAsync()
    {
        var veiculos = await _repository.GetAllAsync();
        return Result<IEnumerable<VeiculoClienteListaDTO>>.Ok(
            veiculos.Select(VeiculoClienteMapping.ToListaDto)
        );
    }

    /*
        metodo de busca de veiculoCliente pelo Cliente 
        vinculado valida que caso Cliente buscado 
        seja vazio retorna o aviso que não foi encontrado
    */
    public async Task<Result<IEnumerable<VeiculoClienteListaDTO>>> ObterPorClienteAsync(Guid clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);

        if (cliente is null)
            return Result<IEnumerable<VeiculoClienteListaDTO>>.Fail("Cliente não encontrado");

        var veiculos = await _repository.ObterPorClienteAsync(clienteId);
        return Result<IEnumerable<VeiculoClienteListaDTO>>.Ok(
            veiculos.Select(VeiculoClienteMapping.ToListaDto)
        );
    }

    /*
        metodo para criar novo VeiculoCliente
        verifica que o cliente existe fazendo
        uma busca por id, caso seja vazio a 
        criação do VeiculoCliente falha.
    */
    public async Task<Result<Guid>> AddAsync(CriarVeiculoClienteDTO dto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);

        if (cliente is null)
            return Result<Guid>.Fail("Cliente não encontrado");

        try
        {
            var veiculo = VeiculoClienteMapping.ToEntity(dto);

            await _repository.AddAsync(veiculo);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(veiculo.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao cadastrar veículo: {ex.Message}");
        }
    }

    /*
        metodo que atualiza veiculoCliente ja existente
        faz busca por id e caso veiculoCliente seja vazio 
        ele retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(AtualizarVeiculoClienteDTO dto)
    {
        var veiculo = await _repository.GetByIdAsync(dto.Id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        try
        {
            veiculo.AtualizarDadosVeiculoCliente(
                dto.Marca,
                dto.Modelo,
                dto.Cor,
                dto.Ano
            );

            _repository.Update(veiculo);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que remove veiculoCliente 
        por id caso seja vazio retona 
        o aviso que nao foi encontrado
    */
    public async Task<Result> RemoveAsync(Guid id)
    {
        var veiculo = await _repository.GetByIdAsync(id);

        if (veiculo is null)
            return Result.Fail("Veículo não encontrado");

        _repository.Remove(veiculo);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}