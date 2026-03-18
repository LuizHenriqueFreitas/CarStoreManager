using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.DTOs;

public record VeiculoDTO
(
    Guid Id,
    string Marca,
    string Modelo,
    int Ano,
    string Cor,
    int Quilometragem,
    string Estado,
    PlacaVeiculo Placa,
    decimal Valor,
    bool Disponivel
);