using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class VendedorMapping
{
    public static VendedorDTO ToDto(Vendedor entity)
    {
        return new VendedorDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone(),
            Nivel = entity.GetNivel(),
            DataContratacao = entity.GetDataContratacao(),
            AnosEmpresa = entity.DadosFuncionario.CalcularAnosEmpresa()
        };
    }

    public static VendedorListaDTO ToListaDto(Vendedor entity)
    {
        return new VendedorListaDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Telefone = entity.GetTelefone(),
            Nivel = entity.GetNivel()
        };
    }

    public static Vendedor ToEntity(CriarVendedorDTO dto, string senhaHash)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException($"Nível inválido: {dto.Nivel}");

        return new Vendedor(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            senhaHash,
            dto.Salario,
            nivel,
            dto.DataContratacao
        );
    }

    public static void UpdateEntity(Vendedor entity, AtualizarVendedorDTO dto)
    {
        entity.AtualizarEmail(dto.Email);
        entity.AtualizarTelefone(dto.Telefone);
    }
}