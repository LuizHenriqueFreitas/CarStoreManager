using CarStoreManager.Application.DTOs.Recepcionista;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Application.Mappings.Shared;

public static class RecepcionistaMapping
{
    public static RecepcionistaDTO ToDto(Domain.Entities.Recepcionista entity)
    {
        return new RecepcionistaDTO
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

    public static RecepcionistaListaDTO ToListaDto(Domain.Entities.Recepcionista entity)
    {
        return new RecepcionistaListaDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Telefone = entity.GetTelefone(),
            Nivel = entity.GetNivel()
        };
    }

    public static Domain.Entities.Recepcionista ToEntity(CriarRecepcionistaDTO dto, string senhaHash)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException($"Nível inválido: {dto.Nivel}");

        return new Domain.Entities.Recepcionista(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            senhaHash,
            nivel,
            dto.DataContratacao
        );
    }

    public static void UpdateEntity(Domain.Entities.Recepcionista entity, AtualizarRecepcionistaDTO dto)
    {
        entity.AtualizarEmail(dto.Email);
        entity.AtualizarTelefone(dto.Telefone);

        if (Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            entity.AtualizarNivel(nivel);
    }
}
