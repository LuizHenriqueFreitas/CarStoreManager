using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.Interfaces;

public interface IOrdemServicoService
{
    // =========================
    // CONSULTAS
    // =========================

    Task<Result<OrdemServicoDTO>> ObterPorIdAsync(Guid id);

    Task<Result<IEnumerable<OrdemServicoListaDTO>>> ObterTodasAsync();


    // =========================
    // CRIAÇÃO
    // =========================

    Task<Result<Guid>> CriarAsync(CriarOrdemServicoDTO dto);


    // =========================
    // ITENS (CORE DA OFICINA)
    // =========================

    Task<Result> AdicionarItemAsync(AdicionarItemOrdemServicoDTO dto);

    Task<Result> RemoverItemAsync(Guid ordemId, Guid itemId);

    Task<Result> AtualizarItemAsync(AtualizarItemOrdemServicoDTO dto);


    // =========================
    // STATUS / FLUXO
    // =========================

    Task<Result> AtualizarStatusAsync(AtualizarOrdemServicoDTO dto);


    // =========================
    // CÁLCULOS
    // =========================

    Task<Result> RecalcularValoresAsync(Guid ordemId);
}