using System.Data;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class MecanicoMapping
{
    // =========================
    // ENTITY → DTO (DETALHE)
    // =========================
    public static MecanicoDTO ToDto(Mecanico entity)
    {
        return new MecanicoDTO
        {
            Id= entity.Id,
            Nome = entity.Nome,
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone(),
            Especialidade = entity.GetEspecialidade(),
            ValorHora = entity.GetValorHora(),
            NivelOcupacao = entity.GetNivelOcupacao()
        };
    }

    // =========================
    // ENTITY → DTO (LISTA)
    // =========================
    public static MecanicoListaDTO ToListaDto(Mecanico entity)
    {
        return new MecanicoListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Especialidade = entity.Especialidade.ToString(),
            NivelOcupacao = entity.NivelOcupacao.ToString()
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static Mecanico ToEntity(CriarMecanicoDTO dto)
    {
        return new Mecanico(
            dto.Nome,
            new Email(dto.Email),
            new Telefone(dto.Telefone),
            ConverterEspecialidade(dto.Especialidade),
            new Dinheiro(dto.ValorHora)
        );
    }

    // =========================
    // UPDATE
    // =========================
    public static void UpdateEntity(Mecanico entity, AtualizarMecanicoDTO dto)
    {
        entity.AtualizarDados(
            dto.Nome,
            new Email(dto.Email),
            new Telefone(dto.Telefone),
            ConverterEspecialidade(dto.Especialidade),
            new Dinheiro(dto.ValorHora)
        );
    }

    // =========================
    // HELPERS
    // =========================
    private static EspecialidadeMecanico ConverterEspecialidade(string valor)
    {
        if (!Enum.TryParse<EspecialidadeMecanico>(valor, true, out var resultado))
            throw new ArgumentException($"Especialidade inválida: {valor}");

        return resultado;
    }
}