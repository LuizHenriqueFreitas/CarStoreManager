using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Fornecedor;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _repository;

    public FornecedorService(IFornecedorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FornecedorDTO>> GetByIdAsync(Guid id)
    {
        var fornecedor = await _repository.GetByIdAsync(id);

        if (fornecedor is null)
            return Result<FornecedorDTO>.Fail("Fornecedor não encontrado");

        return Result<FornecedorDTO>.Ok(FornecedorMapping.ToDto(fornecedor));
    }

    public async Task<Result<IEnumerable<FornecedorListaDTO>>> GetAllAsync()
    {
        var fornecedores = await _repository.GetAllAsync();

        var lista = fornecedores.Select(FornecedorMapping.ToListaDto);

        return Result<IEnumerable<FornecedorListaDTO>>.Ok(lista);
    }

    public async Task<Result<Guid>> AddAsync(CriarFornecedorDTO dto)
    {
        if (await _repository.CnpjExisteAsync(dto.Cnpj))
            return Result<Guid>.Fail("CNPJ já cadastrado");

        try
        {
            var fornecedor = FornecedorMapping.ToEntity(dto);

            await _repository.AddAsync(fornecedor);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(fornecedor.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar fornecedor: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(AtualizarFornecedorDTO dto)
    {
        var fornecedor = await _repository.GetByIdAsync(dto.Id);

        if (fornecedor is null)
            return Result.Fail("Fornecedor não encontrado");

        try
        {
            FornecedorMapping.UpdateEntity(fornecedor, dto);

            _repository.Update(fornecedor);
            await _repository.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> AtivarAsync(Guid id)
    {
        var fornecedor = await _repository.GetByIdAsync(id);
        if (fornecedor is null)
            return Result.Fail("Fornecedor não encontrado");

        fornecedor.Ativar();
        _repository.Update(fornecedor);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> DesativarAsync(Guid id)
    {
        var fornecedor = await _repository.GetByIdAsync(id);
        if (fornecedor is null)
            return Result.Fail("Fornecedor não encontrado");

        fornecedor.Desativar();
        _repository.Update(fornecedor);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result<IEnumerable<FornecedorBuscaDTO>>> BuscarAsync(string termo, int limite = 20)
    {
        var fornecedores = await _repository.BuscarAsync(termo, limite);

        var resultado = fornecedores.Select(f => new FornecedorBuscaDTO
        {
            Id = f.Id,
            Nome = f.Nome,
            Cnpj = f.Cnpj.ToString()
        });

        return Result<IEnumerable<FornecedorBuscaDTO>>.Ok(resultado);
    }

    public async Task<Result> RemoveAsync(Guid id)
    {
        var fornecedor = await _repository.GetByIdAsync(id);

        if (fornecedor is null)
            return Result.Fail("Fornecedor não encontrado");

        try
        {
            _repository.Remove(fornecedor);
            await _repository.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível excluir o fornecedor. Ele pode estar vinculado a algum registro — considere desativá-lo em vez de excluir.");
        }
    }
}
