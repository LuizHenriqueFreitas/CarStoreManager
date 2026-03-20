using CarStoreManager.Application.DTOs;
using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Application.Mappings;

public static class ClienteMapping 
{
    public static ClienteDTO ToDTO(Cliente cliente)
    {
        return new ClienteDTO(
            cliente.Id,
            cliente.Nome,
            cliente.Documento,
            cliente.Telefone,
            cliente.Email
        );
    }

    public static Cliente ToEntity(ClienteDTO dto)
    {
        return new Cliente(
            dto.Nome,
            dto.Documento,
            dto.Telefone,
            dto.Email
        );
    }
}