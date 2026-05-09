using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

/*
    Esta arquivo contem a declaração dos atributos e tambem
    dos metodos da Classe de VendedorService.cs.

    Esta classe tem testes automaticos implementados para:
        nada ainda
*/

public class VendedorService : IVendedorService
{
    private readonly IVendedorRepository _repository;

    public VendedorService(IVendedorRepository repository)
    {
        _repository = repository;
    }

    /*
        metodo de busca por id valida que
        caso vendedor buscado seja vazio
        retorna o aviso que não foi encontrado
    */
    public async Task<Result<VendedorDTO>> GetByIdAsync(Guid id)
    {
        var vendedor = await _repository.GetByIdAsync(id);
        if (vendedor is null)
            return Result<VendedorDTO>.Fail("Vendedor não encontrado");

        return Result<VendedorDTO>.Ok(VendedorMapping.ToDto(vendedor));
    }

    //busca todos os vendedores
    public async Task<Result<IEnumerable<VendedorListaDTO>>> GetAllAsync()
    {
        var vendedores = await _repository.GetAllAsync();
        return Result<IEnumerable<VendedorListaDTO>>.Ok(
            vendedores.Select(VendedorMapping.ToListaDto));
    }

    //metodo para criar novo vendedor
    public async Task<Result<Guid>> AddAsync(CriarVendedorDTO dto)
    {
        try
        {
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            var vendedor = VendedorMapping.ToEntity(dto, senhaHash);

            await _repository.AddAsync(vendedor);
            await _repository.SaveChangesAsync();
            return Result<Guid>.Ok(vendedor.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar vendedor: {ex.Message}");
        }
    }

    /*
        metodo que atualiza vendedor ja existente
        faz busca por id e caso vendedor seja vazio 
        ele retona o aviso que nao foi encontrado
    */
    public async Task<Result> UpdateAsync(AtualizarVendedorDTO dto)
    {
        var vendedor = await _repository.GetByIdAsync(dto.Id);
        if (vendedor is null)
            return Result.Fail("Vendedor não encontrado");

        try
        {
            VendedorMapping.UpdateEntity(vendedor, dto);
            _repository.Update(vendedor);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /*
        metodo que remove vendedor por id
        caso seja vazio retona 
        o aviso que nao foi encontrado
    */
    public async Task<Result> RemoveAsync(Guid id)
    {
        var vendedor = await _repository.GetByIdAsync(id);
        if (vendedor is null)
            return Result.Fail("Vendedor não encontrado");

        _repository.Remove(vendedor);
        await _repository.SaveChangesAsync();
        return Result.Ok();
    }
}