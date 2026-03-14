using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoService
{
    Task<Result<IEnumerable<VeiculoDTO>>> ObterTodosAsync ();

    Task<Result<VeiculoDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<VeiculoDTO>>> ObterPorClienteAsync(Guid clienteId);

    Task<Result> CriarAsync(VeiculoDTO veiculo);
    Task<Result> AtualizarAsync(VeiculoDTO veiculo);
    Task<Result> RemoverAsync(Guid id);
}