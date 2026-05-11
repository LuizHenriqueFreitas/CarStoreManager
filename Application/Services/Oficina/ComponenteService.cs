using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;

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
    public async Task<Result<IEnumerable<ComponenteListaDTO>>> GetAllAsync()
    {
        var componentes = await _repository.GetAllAsync();

        var lista = componentes
            .Select(ComponenteMapping.ToListaDto);

        return Result<IEnumerable<ComponenteListaDTO>>.Ok(lista);
    }

    /*
        Estoque agora vive em EstoqueComponente (entidade separada);
        depende de IEstoqueRepository ainda não implementado.
    */
    public Task<Result<IEnumerable<ComponenteDTO>>> ObterComEstoqueBaixoAsync()
        => Task.FromResult(Result<IEnumerable<ComponenteDTO>>.Fail(
            "ObterComEstoqueBaixoAsync ainda não implementado — depende de IEstoqueRepository."));

    /*
        Componente atualmente não tem propriedade Sistema; o filtro por
        SistemaComponente exige primeiro adicionar essa coluna na entidade.
    */
    public Task<Result<IEnumerable<ComponenteDTO>>> ObterPorSistemaAsync(string sistema)
        => Task.FromResult(Result<IEnumerable<ComponenteDTO>>.Fail(
            "ObterPorSistemaAsync ainda não implementado — Componente precisa do campo Sistema."));

    public async Task<Result<Guid>> AddAsync(CriarComponenteDTO dto)
    {
        try
        {
            var componente = ComponenteMapping.FromCriarDto(dto);
            await _repository.AddAsync(componente);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(componente.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar componente: {ex.Message}");
        }
    }

    /*
        metodo que atualiza componente ja existente
        faz busca por id e caso componente seja vazio 
        retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(AtualizarComponenteDTO dto)
    {
        var componente = await _repository.GetByIdAsync(dto.Id);

        if (componente is null)
            return Result.Fail("Componente não encontrado");

        try
        {
            ComponenteMapping.ApplyAtualizarDto(componente, dto);
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

    // Estoque vive em EstoqueComponente (entidade separada).
    // EntradaEstoque/SaidaEstoque foram desativados aqui até IEstoqueRepository existir.
    public Task<Result> EntradaEstoqueAsync(Guid id, int quantidade)
        => Task.FromResult(Result.Fail("EntradaEstoqueAsync ainda não implementado — depende de IEstoqueRepository."));

    public Task<Result> SaidaEstoqueAsync(Guid id, int quantidade)
        => Task.FromResult(Result.Fail("SaidaEstoqueAsync ainda não implementado — depende de IEstoqueRepository."));

    /// <summary>
    /// Encontra peças que servem como substituto pelo mesmo CodigoOEM
    /// (cross-brand). Ex.: pastilha Bosch e pastilha Fras-le com o mesmo
    /// OEM da montadora — qualquer marca com aquele OEM serve.
    /// O próprio componente é excluído da lista.
    /// </summary>
    public async Task<Result<List<ComponenteListaDTO>>> ObterEquivalentesAsync(Guid componenteId)
    {
        var alvo = await _repository.GetByIdAsync(componenteId);
        if (alvo is null)
            return Result<List<ComponenteListaDTO>>.Fail("Componente não encontrado");

        if (string.IsNullOrWhiteSpace(alvo.CodigoOEM))
            return Result<List<ComponenteListaDTO>>.Ok(new List<ComponenteListaDTO>());

        var todos = await _repository.GetAllAsync();
        var equivalentes = todos
            .Where(c =>
                c.Id != alvo.Id
                && c.Ativo
                && !string.IsNullOrWhiteSpace(c.CodigoOEM)
                && string.Equals(c.CodigoOEM, alvo.CodigoOEM, StringComparison.OrdinalIgnoreCase))
            .Select(ComponenteMapping.ToListaDto)
            .ToList();

        return Result<List<ComponenteListaDTO>>.Ok(equivalentes);
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