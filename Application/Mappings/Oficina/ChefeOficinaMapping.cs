using CarStoreManager.Application.DTOs.Oficina.ChefeOficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class ChefeOficinaMapping
{
    public static ChefeOficinaDTO ToDto(Domain.Entities.Oficina.ChefeOficina entity)
    {
        return new ChefeOficinaDTO
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

    public static ChefeOficinaListaDTO ToListaDto(Domain.Entities.Oficina.ChefeOficina entity)
    {
        return new ChefeOficinaListaDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Telefone = entity.GetTelefone(),
            Nivel = entity.GetNivel()
        };
    }

    public static Domain.Entities.Oficina.ChefeOficina ToEntity(CriarChefeOficinaDTO dto, string senhaHash)
    {
        if (!Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            throw new ArgumentException($"Nível inválido: {dto.Nivel}");

        return new Domain.Entities.Oficina.ChefeOficina(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            senhaHash,
            nivel,
            dto.DataContratacao
        );
    }

    public static void UpdateEntity(Domain.Entities.Oficina.ChefeOficina entity, AtualizarChefeOficinaDTO dto)
    {
        entity.AtualizarEmail(dto.Email);
        entity.AtualizarTelefone(dto.Telefone);

        if (Enum.TryParse<NivelFuncionario>(dto.Nivel, true, out var nivel))
            entity.AtualizarNivel(nivel);
    }
}
