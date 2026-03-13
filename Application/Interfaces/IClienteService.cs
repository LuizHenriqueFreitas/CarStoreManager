using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.Interfaces;

public interface IClienteService
{
    Task<Result<ClienteDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<ClienteDTO>>> ObterTodosAsync();

    Task<Result> CriarAsync(ClienteDTO cliente);
    Task<Result> AtualizarAsync(ClienteDTO cliente);
    Task<Result> RemoverAsync(Guid id);
}