using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;

namespace CarStoreManager.Application.Interfaces;

public interface IClienteService
{
    Task<Result<ClienteDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<ClienteListaDTO>>> ObterTodosAsync();
    Task<Result<ClienteDTO>> ObterPorCpfAsync(string cpf);
    Task<Result<Guid>> CriarAsync(CriarClienteDTO dto);
    Task<Result> AtualizarAsync(AtualizarClienteDTO dto);
    Task<Result> RemoverAsync(Guid id);
}