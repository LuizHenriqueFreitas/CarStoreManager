using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.ValueObjects;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class PropostaVendaMapping
{
    // =========================
    // ENTITY → DETALHE
    // =========================

    public static PropostaVendaDTO ToDto(PropostaVenda entity)
    {
        return new PropostaVendaDTO
        {
            Id = entity.Id,
            VendedorId = entity.VendedorId,
            VeiculoId = entity.VeiculoId,
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


    // =========================
    // ENTITY → LISTA (TABELA)
    // =========================

    public static PropostaVendaListaDTO ToListaDto(PropostaVenda entity)
    {
        return new PropostaVendaListaDTO
        {
            Id = entity.Id,
            //nome do cliente
            //modelo do carro
            ValorFinal = entity.ValorFinal.Valor,
            Status = entity.Status.ToString(),
        };
    }


    // =========================
    // DTO → ENTITY (CRIAÇÃO)
    // =========================

    public static PropostaVenda ToEntity(CriarPropostaVendaDTO dto)
    {
        return new PropostaVenda(
            dto.VendedorId,
            dto.VeiculoId,
            dto.ClienteId,
            new Dinheiro(dto.ValorBase)
        );
    }


    // =========================
    // MÉTODOS AUXILIARES (OPCIONAL)
    // =========================

    public static IEnumerable<PropostaVendaListaDTO> ToListaDtoList(IEnumerable<PropostaVenda> entities)
    {
        return entities.Select(ToListaDto);
    }
}