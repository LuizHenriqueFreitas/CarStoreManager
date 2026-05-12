using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda.Pagamento;

namespace CarStoreManager.Application.Interfaces;

public interface IPagamentoPropostaService
{
    Task<Result<PagamentoPropostaDTO>> RegistrarPagamentoAsync(
        Guid propostaId, Guid recebidoPor, RegistrarPagamentoPropostaDTO dto);

    Task<Result<ResumoPagamentoPropostaDTO>> ObterResumoAsync(Guid propostaId);

    Task<Result> EstornarPagamentoAsync(Guid pagamentoId);
}
