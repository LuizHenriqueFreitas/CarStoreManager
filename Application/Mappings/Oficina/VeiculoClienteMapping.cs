using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Oficina;

public static class VeiculoClienteMapping
{
    public static VeiculoClienteDTO ToDto(VeiculoCliente entity)
    {
        return new VeiculoClienteDTO
        {
            Id = entity.Id,
            ClienteId = entity.ClienteId,
            Marca = entity.Marca,
            Modelo = entity.Modelo,
            Cor = entity.Cor,
            Ano = entity.GetAno(),
            Descricao = entity.GetDescricao()
        };
    }

    public static VeiculoClienteListaDTO ToListaDto(VeiculoCliente entity)
    {
        return new VeiculoClienteListaDTO
        {
            Id = entity.Id,
            ClienteId = entity.ClienteId,
            Descricao = entity.GetDescricao(),
            Cor = entity.Cor
        };
    }

    public static VeiculoCliente ToEntity(CriarVeiculoClienteDTO dto)
    {
        return new VeiculoCliente(
            dto.ClienteId,
            dto.Marca,
            dto.Modelo,
            dto.Cor,
            new Ano(dto.Ano)
        );
    }
}