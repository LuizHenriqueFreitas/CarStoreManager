namespace CarStoreManager.Application.DTOs;

public record PropostaVendaDTO
(
    Guid Id,
    Guid VendedorId,
    Guid VeiculoId,
    Guid ClienteId,
    decimal ValorBase,
    decimal Desconto,
    decimal ValorFinal,
    decimal Entrada,
    decimal ValorFinanciado,
    int Parcelas,
    DateTime DataCriacao,
    string Status
);