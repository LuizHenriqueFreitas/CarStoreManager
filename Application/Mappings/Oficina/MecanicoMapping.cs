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
            Nome = entity.Nome,
            Email = entity.GetEmail(),
            Telefone = entity.GetTelefone(),
            Especialidade = entity.Especialidade.ToString(),
            Nivel = entity.DadosFuncionario.Nivel.ToString(),
            DataContratacao = entity.DadosFuncionario.DataContratacao
        };
    }

    public static MecanicoListaDTO ToListaDto(Mecanico entity)
    {
        return new MecanicoListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Especialidade = entity.Especialidade.ToString(),
            Nivel = entity.DadosFuncionario.Nivel.ToString()
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
            ConverterEspecialidade(dto.Especialidade),
            ConverterNivel(dto.Nivel),
            DateTime.UtcNow
        );
    }

    public static void UpdateEntity(Mecanico entity, AtualizarMecanicoDTO dto)
    {
        entity.AlterarEspecialidade(
            ConverterEspecialidade(dto.Especialidade)
        );

        entity.AtualizarDadosFuncionario(
            ConverterNivel(dto.Nivel),
            entity.DadosFuncionario.DataContratacao
        );
    }

    private static EspecialidadeMecanico ConverterEspecialidade(string valor)
    {
        if (!Enum.TryParse(valor, true, out EspecialidadeMecanico result))
            throw new ArgumentException("Especialidade inválida");

        return result;
    }

    private static NivelFuncionario ConverterNivel(string valor)
    {
        if (!Enum.TryParse(valor, true, out NivelFuncionario result))
            throw new ArgumentException("Nível inválido");

        return result;
    }
}