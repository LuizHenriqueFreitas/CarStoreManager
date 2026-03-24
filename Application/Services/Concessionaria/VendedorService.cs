using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Mappings.Concessionaria;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Services;

public class VendedorService : IVendedorService
{
    private readonly IVendedorRepository _repository;

    public VendedorService(IVendedorRepository repository)
    {
        _repository = repository;
    }

    // =========================
    // CONSULTAS
    // =========================

    public async Task<Result<VendedorDTO>> ObterPorIdAsync(Guid id)
    {
        var vendedor = await _repository.GetByIdAsync(id);

        if (vendedor is null)
            return Result<VendedorDTO>.Fail("Vendedor não encontrado");

        return Result<VendedorDTO>.Ok(
            VendedorMapping.ToDto(vendedor)
        );
    }

    public async Task<Result<IEnumerable<VendedorListaDTO>>> ObterTodosAsync()
    {
        var vendedores = await _repository.GetAllAsync();

        var lista = vendedores
            .Select(VendedorMapping.ToListaDto);

        return Result<IEnumerable<VendedorListaDTO>>.Ok(lista);
    }

    // =========================
    // CRIAÇÃO
    // =========================

    public async Task<Result<Guid>> CriarAsync(CriarVendedorDTO dto)
    {
        try
        {
            var vendedor = new Vendedor(
                dto.Nome,
                new Email (dto.Email),
                new Telefone (dto.Telefone)
            );

            await _repository.AddAsync(vendedor);
            await _repository.SaveChangesAsync();

            return Result<Guid>.Ok(vendedor.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Erro ao criar vendedor: {ex.Message}");
        }
    }

    // =========================
    // ATUALIZAÇÃO
    // =========================

    public async Task<Result> AtualizarAsync(AtualizarVendedorDto dto)
    {
        var vendedor = await _repository.GetByIdAsync(dto.Id);

        if (vendedor is null)
            return Result.Fail("Vendedor não encontrado");

        try
        {
            vendedor.DefinirNome(dto.Nome);
            vendedor.AtualizarEmail(new Email(dto.Email));
            vendedor.AtualizarTelefone(new Telefone(dto.Telefone));

            _repository.Update(vendedor);
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

    public async Task<Result> RemoverAsync(Guid id)
    {
        var vendedor = await _repository.GetByIdAsync(id);

        if (vendedor is null)
            return Result.Fail("Vendedor não encontrado");

        _repository.Remove(vendedor);
        await _repository.SaveChangesAsync();

        return Result.Ok();
    }
}