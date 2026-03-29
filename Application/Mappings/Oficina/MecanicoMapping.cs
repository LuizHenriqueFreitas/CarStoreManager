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
            Id = entity.GetId(),
            Nome = entity.GetNome(),
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone(),
            Especialidade = entity.GetEspecialidade(),
            Ocupado = entity.GetOcupado(),
            Nivel = entity.GetNivel(),
            DataContratacao = entity.GetDataContratacao()
        };
    }

    public static MecanicoListaDTO ToListaDto(Mecanico entity)
    {
        return new MecanicoListaDTO
        {
            Id = entity.GetId(),
            Nome = entity.GetNome(),
            Especialidade = entity.GetEspecialidade(),
            Nivel = entity.GetNivel()
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
            new Email(dto.Email),
            new Telefone(dto.Telefone),
            BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            ConverterEnum<EspecialidadeMecanico>(dto.Especialidade, "Especialidade"),
            ConverterEnum<NivelFuncionario>(dto.Nivel, "Nivel"),
            DateTime.UtcNow
        );
    }

    public static void UpdateEntity(Mecanico entity, AtualizarMecanicoDTO dto)
    {
        entity.AlterarEspecialidade(
            ConverterEnum<EspecialidadeMecanico>(dto.Especialidade, "Especialidade")
        );

        entity.AtualizarDadosFuncionario(
            ConverterEnum<NivelFuncionario>(dto.Nivel, "Nivel"),
            entity.DadosFuncionario.DataContratacao
        );
    }

    private static T ConverterEnum<T>(string valor, string campo) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(valor, true, out var resultado))
            throw new ArgumentException($"{campo} inválido: {valor}");
        return resultado;
    }
}