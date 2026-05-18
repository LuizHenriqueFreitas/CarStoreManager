using CarStoreManager.Application.DTOs.Oficina.Mecanico;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class MecanicoMapping
{
    public static MecanicoDTO ToDto(Mecanico entity)
    {
        return new MecanicoDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone(),
            Especialidade = entity.GetEspecialidade(),
            Ocupado = entity.GetOcupado(),
            Nivel = entity.GetNivelExperiencia(),
            DataContratacao = entity.GetDataContratacao()
        };
    }

    public static MecanicoListaDTO ToListaDto(Mecanico entity)
    {
        return new MecanicoListaDTO
        {
            Id = entity.Id,
            Nome = entity.GetNome(),
            Especialidade = entity.GetEspecialidade(),
            Nivel = entity.GetNivelExperiencia()
        };
    }

    public static MecanicoLookupDTO ToLookupDto(Mecanico entity)
    {
        return new MecanicoLookupDTO
        {
            Id = entity.Id,
            Nome = entity.Nome
        };
    }

    public static Mecanico ToEntity(CriarMecanicoDTO dto)
    {
        return new Mecanico(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Senha,
            ConverterEnum<EspecialidadeMecanico>(dto.Especialidade, "Especialidade"),
            ConverterEnum<NivelFuncionario>(dto.Nivel, "Nivel"),
            dto.DataContratacao
        );
    }

    public static void UpdateEntity(Mecanico entity, AtualizarMecanicoDTO dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Email))
            entity.AtualizarEmail(dto.Email);
        if (!string.IsNullOrWhiteSpace(dto.Telefone))
            entity.AtualizarTelefone(dto.Telefone);

        if (!string.IsNullOrWhiteSpace(dto.Especialidade))
            entity.AlterarEspecialidade(
                ConverterEnum<EspecialidadeMecanico>(dto.Especialidade, "Especialidade"));

        if (!string.IsNullOrWhiteSpace(dto.Nivel))
            entity.AtualizarDadosFuncionario(
                ConverterEnum<NivelFuncionario>(dto.Nivel, "Nivel"));
    }

    private static T ConverterEnum<T>(string valor, string campo) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(valor, true, out var resultado))
            throw new ArgumentException($"{campo} inválido: {valor}");
        return resultado;
    }
}