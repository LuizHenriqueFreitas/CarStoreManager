using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class PropostaVendaMapping
{
    public static PropostaVendaDTO ToDto(PropostaVenda entity)
    {
        return new PropostaVendaDTO
        {
            Id = entity.Id,
            VendedorId = entity.VendedorId,
            VeiculoVendaId = entity.VeiculoVendaId,
            ClienteId = entity.ClienteId,
            ValorBase = entity.GetValorBase(),
            DescontoPercentual = entity.GetDesconto(),
            ValorFinal = entity.GetValorFinal(),
            Entrada = entity.GetEntrada(),
            ValorFinanciado = entity.GetValorFinanciado(),
            Parcelas = entity.GetParcelas(),
            DataCriacao = entity.DataCriacao,
            Status = entity.Status.ToString()
        };
    }

    public static PropostaVendaListaDTO ToListaDto(PropostaVenda entity)
    {
        return new PropostaVendaListaDTO
        {
            Id = entity.Id,
            ClienteId = entity.ClienteId,
            VeiculoVendaId = entity.VeiculoVendaId,
            ValorFinal = entity.GetValorFinal(),
            Status = entity.Status.ToString(),
            DataCriacao = entity.DataCriacao
        };
    }

    public static PropostaVenda ToEntity(CriarPropostaVendaDTO dto)
    {
        return new PropostaVenda(
            dto.VendedorId,
            dto.VeiculoVendaId,
            dto.ClienteId,
            new Dinheiro(dto.ValorBase)
        );
    }
}