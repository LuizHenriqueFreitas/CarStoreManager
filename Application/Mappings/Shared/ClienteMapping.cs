using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Shared;

public static class ClienteMapping
{
    // =========================
    // ENTITY → DTO (DETALHE)
    // =========================
    public static ClienteDTO ToDto(Cliente entity)
    {
        return new ClienteDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cpf = entity.GetCpf(),
            Telefone = entity.GetTelefone(),
            Email = entity.GetEmail(),
            Endereco = ToEnderecoDto(entity.Endereco)
        };
    }

    // =========================
    // ENTITY → DTO (LISTA)
    // =========================
    public static ClienteListaDTO ToListaDto(Cliente entity)
    {
        return new ClienteListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cpf = entity.GetCpf(),
            Telefone = entity.GetTelefone(),
            Email = entity.GetEmail()
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static Cliente ToEntity(CriarClienteDTO dto)
    {
        return new Cliente(
            dto.Nome,
            dto.Email,
            dto.Telefone,
            dto.Cpf,
            ToEnderecoEntity(dto.Endereco)
        );
    }

    // =========================
    // UPDATE
    // =========================
    public static void UpdateEntity(Cliente entity, AtualizarClienteDTO dto)
    {
        entity.AtualizarClienteDados(
            dto.Nome,
            dto.Email,
            dto.Telefone
        );
        entity.AtualizarClienteEndereco(ToEnderecoEntity(dto.Endereco));
    }

    // =========================
    // ENDERECO
    // =========================
    public static EnderecoDTO ToEnderecoDto(Endereco endereco)
    {
        if (endereco is null)
            return new EnderecoDTO();

        return new EnderecoDTO
        {
            Logradouro = endereco.Logradouro,
            Numero = endereco.Numero,
            Complemento = endereco.Complemento,
            Bairro = endereco.Bairro,
            Cidade = endereco.Cidade,
            Uf = endereco.Uf,
            Cep = endereco.Cep
        };
    }

    public static Endereco ToEnderecoEntity(EnderecoDTO dto)
    {
        if (dto is null)
            throw new ArgumentException("Endereço é obrigatório");

        return new Endereco(
            dto.Logradouro,
            dto.Numero,
            dto.Complemento,
            dto.Bairro,
            dto.Cidade,
            dto.Uf,
            dto.Cep
        );
    }
}
