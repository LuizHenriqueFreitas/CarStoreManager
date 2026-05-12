using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda.Pagamento;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Concessionaria;

public class PagamentoPropostaService : IPagamentoPropostaService
{
    private readonly IPagamentoPropostaRepository _pagamentoRepo;
    private readonly IPropostaVendaRepository _propostaRepo;

    public PagamentoPropostaService(
        IPagamentoPropostaRepository pagamentoRepo,
        IPropostaVendaRepository propostaRepo)
    {
        _pagamentoRepo = pagamentoRepo;
        _propostaRepo = propostaRepo;
    }

    public async Task<Result<PagamentoPropostaDTO>> RegistrarPagamentoAsync(
        Guid propostaId, Guid recebidoPor, RegistrarPagamentoPropostaDTO dto)
    {
        var proposta = await _propostaRepo.GetByIdAsync(propostaId);
        if (proposta is null) return Result<PagamentoPropostaDTO>.Fail("Proposta não encontrada.");

        if (!Enum.TryParse<ModoPagamento>(dto.ModoPagamento, ignoreCase: true, out var modo))
            return Result<PagamentoPropostaDTO>.Fail($"Modo de pagamento inválido: {dto.ModoPagamento}");

        var pagosAtuais = (await _pagamentoRepo.ObterPorPropostaAsync(propostaId))
            .Sum(p => p.Valor.GetValorDinheiro());
        var restante = proposta.GetValorFinal() - pagosAtuais;
        if (dto.Valor > restante)
            return Result<PagamentoPropostaDTO>.Fail(
                $"Valor (R$ {dto.Valor:N2}) ultrapassa o saldo restante da proposta (R$ {restante:N2}).");

        try
        {
            var pagamento = new PagamentoProposta(
                propostaId, modo, dto.Valor, recebidoPor,
                dto.ReferenciaExterna, dto.Observacoes);

            await _pagamentoRepo.AddAsync(pagamento);
            await _pagamentoRepo.SaveChangesAsync();

            return Result<PagamentoPropostaDTO>.Ok(MapToDto(pagamento));
        }
        catch (Exception ex) { return Result<PagamentoPropostaDTO>.Fail(ex.Message); }
    }

    public async Task<Result<ResumoPagamentoPropostaDTO>> ObterResumoAsync(Guid propostaId)
    {
        var proposta = await _propostaRepo.GetByIdAsync(propostaId);
        if (proposta is null) return Result<ResumoPagamentoPropostaDTO>.Fail("Proposta não encontrada.");

        var pagamentos = (await _pagamentoRepo.ObterPorPropostaAsync(propostaId)).ToList();
        var pago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
        var total = proposta.GetValorFinal();
        var restante = Math.Max(0m, total - pago);

        var status = restante == 0m && pago > 0m ? "Pago"
                   : pago > 0m ? "Parcial"
                   : "Pendente";

        return Result<ResumoPagamentoPropostaDTO>.Ok(new ResumoPagamentoPropostaDTO
        {
            PropostaVendaId = propostaId,
            ValorTotalProposta = total,
            ValorPago = pago,
            ValorRestante = restante,
            StatusPagamento = status,
            Pagamentos = pagamentos.Select(MapToDto).ToList()
        });
    }

    public async Task<Result> EstornarPagamentoAsync(Guid pagamentoId)
    {
        var pagamento = await _pagamentoRepo.GetByIdAsync(pagamentoId);
        if (pagamento is null) return Result.Fail("Pagamento não encontrado.");

        _pagamentoRepo.Remove(pagamento);
        await _pagamentoRepo.SaveChangesAsync();
        return Result.Ok();
    }

    private static PagamentoPropostaDTO MapToDto(PagamentoProposta p) => new()
    {
        Id = p.Id,
        PropostaVendaId = p.PropostaVendaId,
        ModoPagamento = p.ModoPagamento.ToString(),
        Valor = p.Valor.GetValorDinheiro(),
        DataPagamento = p.DataPagamento,
        RecebidoPor = p.RecebidoPor,
        ReferenciaExterna = p.ReferenciaExterna,
        Observacoes = p.Observacoes
    };
}
