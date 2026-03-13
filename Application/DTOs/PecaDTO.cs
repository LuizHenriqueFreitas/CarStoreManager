namespace CarStoreManager.Application.DTOs;

public record PecaDTO
(
    Guid Id,
    string Nome,
    string Modelo,
    decimal Valor,
    int QuantidadeEstoque
);