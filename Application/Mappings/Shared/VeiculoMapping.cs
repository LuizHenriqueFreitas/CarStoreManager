using CarStoreManager.Application.DTOs.Shared.Veiculo;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class VeiculoMapping
{
    // =========================
    // ENTITY → DTO (DETALHE)
    // =========================
    public static VeiculoDTO ToDto(VeiculoVenda entity)
    {
        return new VeiculoDTO
        {
            Id = entity.Id,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Cor = entity.Cor,
            Ano = entity.GetAno(),
            Quilometragem = entity.GetQuilometragem(),
            Estado = entity.Estado.ToString(),
            Placa = entity.GetPlaca(),
            Valor = entity.GetValor(),
            Disponibilidade = entity.Disponibilidade.ToString()
        };
    }

    // =========================
    // ENTITY → DTO (LISTA)
    // =========================
    public static VeiculoListaDTO ToListaDto(VeiculoVenda entity)
    {
        return new VeiculoListaDTO
        {
            Id = entity.Id,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Ano = entity.Ano.Valor,
            Valor = entity.GetValor(),
            Disponibilidade = entity.Disponibilidade.ToString()
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static VeiculoVenda ToEntity(CriarVeiculoDTO dto)
    {
        return new VeiculoVenda(
            Guid.NewGuid(),
            dto.Marca,
            dto.Modelo,
            dto.Cor,
            new Ano(dto.Ano),
            new Quilometragem(dto.Quilometragem),
            new PlacaVeiculo(dto.Placa),
            ConverterEstado(dto.Estado),
            ConverterDisponibilidade(dto.Disponibilidade),
            new Dinheiro(dto.Valor)
        );
    }

    // =========================
    // UPDATE
    // =========================
    public static void UpdateEntity(VeiculoVenda entity, AtualizarVeiculoDTO dto)
    {
        entity.AtualizarDados(
            dto.Marca,
            dto.Modelo,
            dto.Cor,
            new Dinheiro(dto.Valor),
            ConverterDisponibilidade(dto.Disponibilidade),
            dto.Quilometragem,
            ConverterEstado(dto.Estado)
        );
    }

    // =========================
    // HELPERS
    // =========================
    private static EstadoConservacao ConverterEstado(string valor)
    {
        if (!Enum.TryParse<EstadoConservacao>(valor, true, out var resultado))
            throw new ArgumentException($"Estado inválido: {valor}");

        return resultado;
    }

    private static DisponibilidadeVeiculo ConverterDisponibilidade(string valor)
    {
        if (!Enum.TryParse<DisponibilidadeVeiculo>(valor, true, out var resultado))
            throw new ArgumentException($"Disponibilidade inválida: {valor}");

        return resultado;
    }
}