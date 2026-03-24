using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

namespace CarStoreManager.Application.Interfaces;

public interface IPropostaVendaService
{
    // =========================
    // CONSULTAS
    // =========================

    Task<Result<PropostaVendaDTO>> ObterPorIdAsync(Guid id);

    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterTodasAsync();


    // =========================
    // CRIAÇÃO
    // =========================

    Task<Result<Guid>> CriarAsync(CriarPropostaVendaDTO dto);


    // =========================
    // REGRAS DE NEGÓCIO (CORE)
    // =========================

    Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto);

    Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto);

    Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto);


    // =========================
    // FLUXO / STATUS
    // =========================

    Task<Result> AprovarAsync(Guid propostaId);

    Task<Result> RejeitarAsync(Guid propostaId);

    Task<Result> CancelarAsync(Guid propostaId);
}