using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using Oficina.Domain.Entities;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de ComponenteService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class ComponenteService : IComponenteService
{
    private readonly IComponenteRepository _repository;

    public ComponenteService(IComponenteRepository repository)
    {
        _repository = repository;
    }

    /* ======================
        metodos de PESQUISA
     =======================*/

    /*
        metodo de busca por id valida que
        caso componente buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<ComponenteDTO>> GetByIdAsync(Guid id)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result<ComponenteDTO>.Fail("Componente não encontrado");

        return Result<ComponenteDTO>.Ok(
            ComponenteMapping.ToDto(componente)
        );
    }

    //busca todos os componentes
    public async Task<Result<IEnumerable<ComponenteDTO>>> GetAllAsync()
    {
        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Select(ComponenteMapping.ToDto);

        return Result<IEnumerable<ComponenteDTO>>.Ok(lista);
    }

    /*
        metodo que busca os componentes com estoque baixo
        caso o componente buscado por id seja vazio
        ele retorna o aviso que o componente nao foi encontrado
    *//*
    public async Task<Result<IEnumerable<ComponenteDTO>>> ObterComEstoqueBaixoAsync()
    {
        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Where(c => c.EstoqueBaixo())
            .Select(ComponenteMapping.ToListaDto);

        return Result<IEnumerable<ComponenteDTO>>.Ok(lista);
    }*/

    /*
        metodo que filtra os componentes de 
        acordo com o sistema que fazem parte
        retorna falha caso o sistema informado seja invalido
    *//*
    public async Task<Result<IEnumerable<ComponenteDTO>>> ObterPorSistemaAsync(string sistema)
    {
        if (!Enum.TryParse<SistemaComponente>(sistema, true, out var sistemaEnum))
            return Result<IEnumerable<ComponenteDTO>>.Fail("Sistema inválido");

        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Where(c => c.Sistema == sistemaEnum)
            .Select(ComponenteMapping.ToDto);

        return Result<IEnumerable<ComponenteDTO>>.Ok(lista);
    }*/

    //metodo para criar novo componente
    /*
    public async Task<Result<Guid>> AddAsync(ComponenteDTO dto)
    {
        if(!Enum.TryParse<SistemaComponente>(dto.Sistema, true, out var sistema))
            return Result<Guid>.Fail("Sistema inválido");
        try
        {
            var componente = new Componente(
                //implementar
            );

            await _repository.AddAsync(componente);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(componente.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar componente: {ex.Message}");
        }
    }*/

    /*
        metodo que atualiza componente ja existente
        faz busca por id e caso componente seja vazio 
        retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(ComponenteDTO dto)
    {
        var componente = await _repository.GetByIdAsync(dto.Id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            componente.AtualizarDadosComponente(
                dto.Valor,
                dto.QuantidadeEstoque,
                dto.EstoqueMinimo
            );

            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /* ===========================
        gerencimento de ESTOQUE
     ===========================*/

    /*
        metodo para entrada de estoque
        busca o componente por id
        falha caso o componete seja vazio
    */
    public async Task<Result> EntradaEstoqueAsync(Guid id, int quantidade)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            componente.AdicionarEstoque(quantidade);

            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo para saida de estoque
        busca o componente por id
        falha caso o componete seja vazio
    */
    public async Task<Result> SaidaEstoqueAsync(Guid id, int quantidade)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            componente.RemoverEstoque(quantidade);

            _repository.Update(componente);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que remove componente por id
        caso seja vazio retona 
        o aviso que nao foi encontrado
    */
    public async Task<Result> RemoveAsync(Guid id)
    {
        var componente = await _repository.GetByIdAsync(id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        _repository.Remove(componente);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}