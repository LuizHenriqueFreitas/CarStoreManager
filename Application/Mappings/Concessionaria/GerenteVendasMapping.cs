using CarStoreManager.Application.DTOs.Concessionaria.GerenteVendas;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class GerenteVendasMapping
{
    public static GerenteVendasDTO ToDto(Domain.Entities.Concessionaria.GerenteVendas entity)
    {
        return new GerenteVendasDTO
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

    public static GerenteVendasListaDTO ToListaDto(Domain.Entities.Concessionaria.GerenteVendas entity)
    {
        return new GerenteVendasListaDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Telefone = entity.GetTelefone(),
            Nivel = entity.GetNivel()
        };
    }

    public static Domain.Entities.Concessionaria.GerenteVendas ToEntity(CriarGerenteVendasDTO dto, string senhaHash)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException($"Nível inválido: {dto.Nivel}");

        return new Domain.Entities.Concessionaria.GerenteVendas(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            senhaHash,
            nivel,
            dto.DataContratacao
        );
    }

    public static void UpdateEntity(Domain.Entities.Concessionaria.GerenteVendas entity, AtualizarGerenteVendasDTO dto)
    {
        entity.AtualizarEmail(dto.Email);
        entity.AtualizarTelefone(dto.Telefone);

        if (Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            entity.AtualizarNivel(nivel);
    }
}
