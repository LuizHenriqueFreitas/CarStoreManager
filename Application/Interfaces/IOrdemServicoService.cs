using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.Interfaces;

public interface IOrdemServicoService
{
    Task<Result<OrdemServicoDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<OrdemServicoDTO>>> ObterTodasAsync();

    Task<Result> CriarAsync(OrdemServicoDTO ordem);
    Task<Result> AtualizarStatusAsync(Guid id, string status);
}