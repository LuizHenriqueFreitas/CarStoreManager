using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico.Pagamento;

namespace CarStoreManager.Application.Interfaces;

public interface IPagamentoOrdemServicoService
{
    Task<Result<PagamentoDTO>> RegistrarPagamentoAsync(Guid ordemId, Guid recebidoPor, RegistrarPagamentoDTO dto);
    Task<Result<ResumoPagamentoOrdemDTO>> ObterResumoAsync(Guid ordemId);
    Task<Result> EstornarPagamentoAsync(Guid pagamentoId);
}
