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
        return new ClienteDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cpf = entity.GetCpf(),
            Telefone = entity.GetTelefone(),
            Email = entity.GetEmail()
        };
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
            Telefone = entity.GetTelefone(),
            Email = entity.GetEmail()
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static Cliente ToEntity(CriarClienteDTO dto)
    {
        return new Cliente(
            dto.Nome,
            dto.Cpf,
            dto.Telefone,
            dto.Email
        );
    }

    // =========================
    // UPDATE
    // =========================
    public static void UpdateEntity(Cliente entity, AtualizarClienteDTO dto)
    {
        entity.AtualizarClienteDados(
            dto.Nome,
            dto.Telefone,
            dto.Email
        );
    }
}