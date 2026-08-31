using CarStoreManager.Application.DTOs.Oficina.Fornecedor;
using CarStoreManager.Application.DTOs.Shared.Cliente;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class FornecedorMapping
{
    // =========================
    // ENTITY → DTO
    // =========================
    public static FornecedorDTO ToDto(Fornecedor entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        return new FornecedorDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cnpj = entity.Cnpj.ToString(),
            Email = entity.Email,
            Telefone = entity.Telefone,
            Endereco = entity.Endereco is null ? null : ToEnderecoDto(entity.Endereco),
            Ativo = entity.Ativo
        };
    }

    public static FornecedorListaDTO ToListaDto(Fornecedor entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        return new FornecedorListaDTO
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cnpj = entity.Cnpj.ToString(),
            Email = entity.Email,
            Telefone = entity.Telefone,
            Ativo = entity.Ativo
        };
    }

    // =========================
    // DTO → ENTITY (CREATE)
    // =========================
    public static Fornecedor ToEntity(CriarFornecedorDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new Fornecedor(
            dto.Nome,
            dto.Cnpj,
            ToEnderecoEntity(dto.Endereco),
            dto.Email,
            dto.Telefone
        );
    }

    // =========================
    // UPDATE (só dados de contato)
    // =========================
    public static void UpdateEntity(Fornecedor entity, AtualizarFornecedorDTO dto)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        entity.AtualizarContato(
            dto.Email,
            dto.Telefone,
            dto.Endereco?.Logradouro,
            dto.Endereco?.Numero,
            dto.Endereco?.Complemento,
            dto.Endereco?.Bairro,
            dto.Endereco?.Cidade,
            dto.Endereco?.Uf,
            dto.Endereco?.Cep
        );
    }

    // =========================
    // ENDERECO
    // =========================
    private static EnderecoDTO ToEnderecoDto(Endereco endereco) => new()
    {
        Logradouro = endereco.Logradouro,
        Numero = endereco.Numero,
        Complemento = endereco.Complemento,
        Bairro = endereco.Bairro,
        Cidade = endereco.Cidade,
        Uf = endereco.Uf,
        Cep = endereco.Cep
    };

    private static Endereco? ToEnderecoEntity(EnderecoDTO? dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Logradouro))
            return null;

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
