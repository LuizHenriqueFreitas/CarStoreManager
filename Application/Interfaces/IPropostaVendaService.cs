using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

namespace CarStoreManager.Application.Interfaces;

public interface IPropostaVendaService
{
    Task<Result<PropostaVendaDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterTodasAsync();
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorVendedorAsync(Guid vendedorId);
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorClienteAsync(Guid clienteId);
    Task<Result<Guid>> CriarAsync(CriarPropostaVendaDTO dto);
    Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto);
    Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto);
    Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto);
    Task<Result> AprovarAsync(Guid propostaId);
    Task<Result> RejeitarAsync(Guid propostaId);
    Task<Result> CancelarAsync(Guid propostaId);
}