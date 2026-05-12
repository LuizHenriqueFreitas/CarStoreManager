using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.Interfaces;

public interface IRequisicaoPecaService
{
    Task<Result<RequisicaoPecaDTO>> AbrirAsync(Guid ordemId, Guid mecanicoId, CriarRequisicaoPecaDTO dto);
    Task<Result<IEnumerable<RequisicaoPecaDTO>>> ListarPendentesAsync();
    Task<Result<IEnumerable<RequisicaoPecaDTO>>> ListarPorOrdemAsync(Guid ordemId);
    Task<Result> AtenderAsync(Guid requisicaoId, Guid resolvidaPor, AtenderRequisicaoDTO dto);
    Task<Result> RejeitarAsync(Guid requisicaoId, Guid resolvidaPor, RejeitarRequisicaoDTO dto);

    /// <summary>
    /// Admin chama quando todas as requisições da OS foram resolvidas — devolve
    /// a OS ao status anterior (Pendente/EmAnalise) para o orçamento prosseguir.
    /// </summary>
    Task<Result> LiberarOrdemAsync(Guid ordemId);
}
