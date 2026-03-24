using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class VendedorMapping
{
    // =========================
    // ENTITY → DETALHE
    // =========================

    public static VendedorDTO ToDto(Vendedor entity)
    {
        return new VendedorDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone()
        };
    }


    // =========================
    // ENTITY → LISTA
    // =========================

    public static VendedorListaDTO ToListaDto(Vendedor entity)
    {
        return new VendedorListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Telefone = entity.GetTelefone()
        };
    }


    // =========================
    // DTO → ENTITY (CRIAÇÃO)
    // =========================

    public static Vendedor ToEntity(CriarVendedorDTO dto)
    {
        return new Vendedor(
            dto.Nome,
            new Email(dto.Email),
            new Telefone(dto.Telefone)
        );
    }


    // =========================
    // UPDATE
    // =========================

    public static void AtualizarEntity(Vendedor entity, AtualizarVendedorDto dto)
    {
        entity.AtualizarDados(
            dto.Nome,
            new Email(dto.Email),
            new Telefone(dto.Telefone)
        );
    }


    // =========================
    // LISTA
    // =========================

    public static IEnumerable<VendedorListaDTO> ToListaDtoList(IEnumerable<Vendedor> entities)
    {
        return entities.Select(ToListaDto);
    }
}