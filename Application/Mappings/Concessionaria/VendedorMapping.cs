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
            Nome = entity.Nome,
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone(),
            Nivel = entity.DadosFuncionario.Nivel.ToString(),
            DataContratacao = entity.DadosFuncionario.DataContratacao,
            AnosEmpresa = entity.DadosFuncionario.GetAnosEmpresa()
        };
    }

    public static VendedorListaDTO ToListaDto(Vendedor entity)
    {
        return new VendedorListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Telefone = entity.GetTelefone(),
            Nivel = entity.DadosFuncionario.Nivel.ToString()
        };
    }

    public static Vendedor ToEntity(CriarVendedorDTO dto, string senhaHash)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException($"Nível inválido: {dto.Nivel}");

        return new Vendedor(
            dto.Nome,
            new Email(dto.Email),
            new Telefone(dto.Telefone),
            senhaHash,
            nivel,
            dto.DataContratacao
        );
    }

    public static void UpdateEntity(Vendedor entity, AtualizarVendedorDTO dto)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException($"Nível inválido: {dto.Nivel}");

        entity.AlterarNome(dto.Nome);
        entity.AlterarEmail(new Email(dto.Email));
        entity.AlterarTelefone(new Telefone(dto.Telefone));
        entity.AtualizarDadosFuncionario(nivel, dto.DataContratacao);
    }
}