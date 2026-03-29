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
            Id = entity.GetId(),
            ClienteId = entity.GetClienteId(),
            Marca = entity.GetMarca(),
            Modelo = entity.GetModelo(),
            Cor = entity.GetCor(),
            Ano = entity.GetAno(),
            HistoricoServicos = entity.GetHistorico()
        };
    }

    public static VeiculoClienteListaDTO ToListaDto(VeiculoCliente entity)
    {
        return new VeiculoClienteListaDTO
        {
            Id = entity.GetId(),
            ClienteId = entity.GetClienteId(),
            Marca = entity.GetMarca(),
            Modelo = entity.GetModelo()
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