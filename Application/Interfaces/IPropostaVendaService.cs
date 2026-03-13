using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.Interfaces;

public interface IPropostaVendaService
{
    Task<Result<PropostaVendaDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<PropostaVendaDTO>>> ObterTodasAsync();

    Task<Result> CriarAsync(PropostaVendaDTO proposta);
    Task<Result> AtualizarStatusAsync(Guid id, string status);
}