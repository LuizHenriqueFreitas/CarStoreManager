using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Application.Mappings.Concessionaria;

public static class PropostaVendaMapping
{
    public static PropostaVendaDTO ToDto(PropostaVenda entity)
    {
        var prazo = entity.DataCriacao.AddDays(PropostaVenda.PrazoValidadeDias);
        var diasRestantes = (int)Math.Ceiling((prazo - DateTime.UtcNow).TotalDays);

        return new PropostaVendaDTO
        {
            Id = entity.Id,
            VendedorId = entity.GetVendedorId(),
            VeiculoVendaId = entity.GetVeiculoId(),
            ClienteId = entity.GetClienteId(),
            ValorBase = entity.GetValorBase(),
            DescontoPercentual = entity.GetDesconto(),
            ValorFinal = entity.GetValorFinal(),
            Entrada = entity.GetEntrada(),
            ValorLiquidoFinanciamento = entity.ValorLiquidoFinanciamento(),
            DataCriacao = entity.GetDataCriacao(),
            Status = entity.Status.ToString(),
            ModoPagamento = entity.ModoPagamento.ToString(),
            DataSolicitacaoFinanciamento = entity.DataSolicitacaoFinanciamento,
            DataRespostaFinanciadora = entity.DataRespostaFinanciadora,
            ParcelasFinanciamento = entity.ParcelasFinanciamento,
            ValorParcela = entity.ValorParcela?.GetValorDinheiro(),
            TaxaJurosMensal = entity.TaxaJurosMensal,
            ObservacoesFinanciamento = entity.ObservacoesFinanciamento,
            DataAprovacao = entity.DataAprovacao,
            MotivoRejeicao = entity.MotivoRejeicao,
            MotivoCancelamento = entity.MotivoCancelamento,
            DiasRestantes = diasRestantes
        };
    }

    public static PropostaVendaListaDTO ToListaDto(PropostaVenda entity)
    {
        return new PropostaVendaListaDTO
        {
            Id = entity.Id,
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

    public static decimal ToDesconto(AplicarDescontoDTO dto) => dto.Percentual;
    public static decimal ToEntrada(DefinirEntradaDTO dto) => dto.ValorEntrada;
    public static int ToParcelas(GerarFinanciamentoDTO dto) => dto.Parcelas;

    public static ModoPagamento ParseModoPagamento(string s)
    {
        if (Enum.TryParse<ModoPagamento>(s, ignoreCase: true, out var m))
            return m;
        throw new ArgumentException($"Modo de pagamento inválido: '{s}'.");
    }
}
