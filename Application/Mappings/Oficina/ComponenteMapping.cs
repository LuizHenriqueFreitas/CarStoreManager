using CarStoreManager.Application.DTOs.Oficina.Componente;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class ComponenteMapping
{
    // =========================
    // ENTITY → DTO (DETALHE)
    // =========================
    public static ComponenteDTO ToDto(Componente entity)
    {
        return new ComponenteDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Modelo = entity.Modelo,
            Valor = entity.GetValor(),
            QuantidadeEstoque = entity.QuantidadeEstoque,
            EstoqueMinimo = entity.EstoqueMinimo,
            Sistema = entity.Sistema.ToString()
        };
    }

    // =========================
    // ENTITY → DTO (LISTA)
    // =========================
    public static ComponenteListaDTO ToListaDto(Componente entity)
    {
        return new ComponenteListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            QuantidadeEstoque = entity.QuantidadeEstoque,
            Sistema = entity.Sistema.ToString()
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static Componente ToEntity(CriarComponenteDTO dto)
    {
        return new Componente(
            dto.Nome,
            dto.Modelo,
            ConverterSistema(dto.Sistema),
            new Dinheiro(dto.Valor),
            dto.QuantidadeEstoque,
            dto.EstoqueMinimo
        );
    }

    // =========================
    // UPDATE
    // =========================
    public static void UpdateEntity(Componente entity, AtualizarComponenteDTO dto)
    {
        entity.AtualizarDados(
            new Dinheiro(dto.Valor),
            dto.QuantidadeEstoque,
            dto.EstoqueMinimo
        );
    }

    // =========================
    // HELPERS
    // =========================
    private static SistemaComponente ConverterSistema(string valor)
    {
        if (!Enum.TryParse<SistemaComponente>(valor, true, out var resultado))
            throw new ArgumentException($"Sistema inválido: {valor}");

        return resultado;
    }
}