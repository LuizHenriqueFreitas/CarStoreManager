namespace CarStoreManager.Application.DTOs;

public record ClienteDTO
(
    Guid Id,
    string Nome,
    string Documento,
    string Telefone,
    string Email
);