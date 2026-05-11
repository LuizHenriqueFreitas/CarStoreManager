using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Application.Services.Oficina;

public class PagamentoOrdemServicoService : IPagamentoOrdemServicoService
{
    private readonly IPagamentoOrdemServicoRepository _pagamentoRepo;
    private readonly IOrdemServicoRepository _ordemRepo;

    public PagamentoOrdemServicoService(
        IPagamentoOrdemServicoRepository pagamentoRepo,
        IOrdemServicoRepository ordemRepo)
    {
        _pagamentoRepo = pagamentoRepo;
        _ordemRepo = ordemRepo;
    }

    public async Task<Result<PagamentoDTO>> RegistrarPagamentoAsync(
        Guid ordemId, Guid recebidoPor, RegistrarPagamentoDTO dto)
    {
        var ordem = await _ordemRepo.GetByIdAsync(ordemId);
        if (ordem is null) return Result<PagamentoDTO>.Fail("Ordem de serviço não encontrada.");

        if (!Enum.TryParse<ModoPagamento>(dto.ModoPagamento, ignoreCase: true, out var modo))
            return Result<PagamentoDTO>.Fail($"Modo de pagamento inválido: {dto.ModoPagamento}");

        // Não permite passar do total da OS — bloqueia por segurança contábil.
        var pagosAtuais = (await _pagamentoRepo.ObterPorOrdemAsync(ordemId))
            .Sum(p => p.Valor.GetValorDinheiro());
        var restante = ordem.GetValorTotal() - pagosAtuais;
        if (dto.Valor > restante)
            return Result<PagamentoDTO>.Fail(
                $"Valor (R$ {dto.Valor:N2}) ultrapassa o saldo restante da OS (R$ {restante:N2}).");

        try
        {
            var pagamento = new PagamentoOrdemServico(
                ordemId, modo, dto.Valor, recebidoPor,
                dto.ReferenciaExterna, dto.Observacoes);

            await _pagamentoRepo.AddAsync(pagamento);
            await _pagamentoRepo.SaveChangesAsync();

            return Result<PagamentoDTO>.Ok(MapToDto(pagamento));
        }
        catch (Exception ex) { return Result<PagamentoDTO>.Fail(ex.Message); }
    }

    public async Task<Result<ResumoPagamentoOrdemDTO>> ObterResumoAsync(Guid ordemId)
    {
        var ordem = await _ordemRepo.GetByIdAsync(ordemId);
        if (ordem is null) return Result<ResumoPagamentoOrdemDTO>.Fail("Ordem não encontrada.");

        var pagamentos = (await _pagamentoRepo.ObterPorOrdemAsync(ordemId)).ToList();
        var pago = pagamentos.Sum(p => p.Valor.GetValorDinheiro());
        var total = ordem.GetValorTotal();
        var restante = Math.Max(0m, total - pago);

        var status = restante == 0m && pago > 0m ? StatusPagamentoOrdemServico.Pago
                   : pago > 0m ? StatusPagamentoOrdemServico.Parcial
                   : StatusPagamentoOrdemServico.Pendente;

        return Result<ResumoPagamentoOrdemDTO>.Ok(new ResumoPagamentoOrdemDTO
        {
            OrdemServicoId = ordemId,
            ValorTotalOrdem = total,
            ValorPago = pago,
            ValorRestante = restante,
            StatusPagamento = status.ToString(),
            Pagamentos = pagamentos.Select(MapToDto).ToList()
        });
    }

    public async Task<Result> EstornarPagamentoAsync(Guid pagamentoId)
    {
        var pagamento = await _pagamentoRepo.GetByIdAsync(pagamentoId);
        if (pagamento is null) return Result.Fail("Pagamento não encontrado.");

        // Estorno = remover. Operação irreversível, registrada via remoção;
        // futuramente pode virar soft-delete + auditoria.
        _pagamentoRepo.Remove(pagamento);
        await _pagamentoRepo.SaveChangesAsync();
        return Result.Ok();
    }

    private static PagamentoDTO MapToDto(PagamentoOrdemServico p) => new()
    {
        Id = p.Id,
        OrdemServicoId = p.OrdemServicoId,
        ModoPagamento = p.ModoPagamento.ToString(),
        Valor = p.Valor.GetValorDinheiro(),
        DataPagamento = p.DataPagamento,
        RecebidoPor = p.RecebidoPor,
        ReferenciaExterna = p.ReferenciaExterna,
        Observacoes = p.Observacoes
    };
}
