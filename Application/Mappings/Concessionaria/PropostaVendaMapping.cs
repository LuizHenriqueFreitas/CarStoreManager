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
            Id = entity.GetId(),
            VendedorId = entity.GetVendedorId(),
            VeiculoVendaId = entity.GetVeiculoId(),
            ClienteId = entity.GetClienteId(),
            ValorBase = entity.GetValorBase(),
            DescontoPercentual = entity.GetDesconto(),
            ValorFinal = entity.GetValorFinal(),
            Entrada = entity.GetEntrada(),
            DataCriacao = entity.GetDataCriacao(),
            Status = entity.Status.ToString()
        };
    }

    public static PropostaVendaListaDTO ToListaDto(PropostaVenda entity)
    {
        return new PropostaVendaListaDTO
        {
            Id = entity.GetId(),
            ClienteId = entity.GetClienteId(),
            VeiculoVendaId = entity.GetVeiculoId(),
            ValorFinal = entity.GetValorFinal(),
            Status = entity.Status.ToString(),
            DataCriacao = entity.GetDataCriacao()
        };
    }

    public static PropostaVenda ToEntity(CriarPropostaVendaDTO dto)
    {
        return new PropostaVenda(
            dto.VendedorId,
            dto.VeiculoVendaId,
            dto.ClienteId,
            dto.ValorBase,
            dto.DescontoPercentual
        );
    }

    public static decimal ToDesconto(AplicarDescontoDTO dto)
        => dto.Percentual;

    public static decimal ToEntrada(DefinirEntradaDTO dto)
        => dto.ValorEntrada;

    public static int ToParcelas(GerarFinanciamentoDTO dto)
        => dto.Parcelas;
}