using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Shared;

public static class ClienteMapping
{
    // =========================
    // ENTITY → DTO (DETALHE)
    // =========================
    public static ClienteDTO ToDto(Cliente entity)
    {
        return new ClienteDTO(
            entity.Id,
            entity.Nome,
            entity.CPF.Numero,
            entity.Telefone.Numero,
            entity.Email
        );
    }

    // =========================
    // ENTITY → DTO (LISTA)
    // =========================
    public static ClienteListaDTO ToListaDto(Cliente entity)
    {
        return new ClienteListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
             = entity.CPF.Numero
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static Cliente ToEntity(CriarClienteDTO dto)
    {
        return new Cliente(
            dto.Nome,
            new Cpf(dto.Cpf),
            new Telefone(dto.Telefone),
            new Email(dto.Email)
        );
    }

    // =========================
    // UPDATE
    // =========================
    public static void UpdateEntity(Cliente entity, AtualizarClienteDTO dto)
    {
        entity.AtualizarDados(
            dto.Nome,
            new Telefone(dto.Telefone),
            new Email (dto.Email)
        );
    }
}