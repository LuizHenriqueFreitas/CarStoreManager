namespace CarStoreManager.Application.DTOs;

public record OrdemServicoDTO
(
    Guid Id,
    Guid ClienteId,
    Guid MecanicoId,
    Guid VeiculoId,
    string Descricao,
    DateTime DataCriacao,
    DateTime PrazoEstimado,
    decimal CustoPecas,
    decimal CustoServico,
    decimal ValorTotal,
    string Status
);