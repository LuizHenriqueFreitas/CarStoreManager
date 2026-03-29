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
            Id = entity.GetId(),
            Nome = entity.GetNome(),
            Modelo = entity.GetModelo(),
            Valor = entity.GetValor(),
            QuantidadeEstoque = entity.GetQuantidade(),
            EstoqueMinimo = entity.GetEstoqueMinimo(),
            Sistema = entity.GetSistema()
        };
    }

    // =========================
    // ENTITY → DTO (LISTA)
    // =========================
    public static ComponenteListaDTO ToListaDto(Componente entity)
    {
        return new ComponenteListaDTO
        {
            Id = entity.GetId(),
            Nome = entity.GetNome(),
            QuantidadeEstoque = entity.GetQuantidade(),
            Sistema = entity.GetSistema()
        };
    }

    // =========================
    // ENTITY → DTO (LOOKUP)
    // =========================
    public static ComponenteLookupDTO ToLookupDto(Componente entity)
    {
        return new ComponenteLookupDTO
        {
            Id = entity.GetId(),
            Nome = entity.GetNome(),
            Modelo = entity.GetModelo(),
            Valor = entity.GetValor(),
            Sistema = entity.GetSistema()
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