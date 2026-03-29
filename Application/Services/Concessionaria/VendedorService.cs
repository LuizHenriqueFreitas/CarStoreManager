using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services;

public class VendedorService : IVendedorService
{
    private readonly IVendedorRepository _repository;

    public VendedorService(IVendedorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<VendedorDTO>> GetByIdAsync(Guid id)
    {
        var vendedor = await _repository.GetByIdAsync(id);
        if (vendedor is null)
            return Result<VendedorDTO>.Fail("Vendedor não encontrado");

        return Result<VendedorDTO>.Ok(VendedorMapping.ToDto(vendedor));
    }

    public async Task<Result<IEnumerable<VendedorListaDTO>>> GetAllAsync()
    {
        var vendedores = await _repository.GetAllAsync();
        return Result<IEnumerable<VendedorListaDTO>>.Ok(
            vendedores.Select(VendedorMapping.ToListaDto));
    }

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