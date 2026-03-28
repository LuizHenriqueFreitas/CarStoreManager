using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoClienteService
{
    Task<Result<VeiculoClienteDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<VeiculoClienteListaDTO>>> ObterTodosAsync();
    Task<Result<IEnumerable<VeiculoClienteListaDTO>>> ObterPorClienteAsync(Guid clienteId);
    Task<Result<Guid>> CriarAsync(CriarVeiculoClienteDTO dto);
    Task<Result> AtualizarAsync(AtualizarVeiculoClienteDTO dto);
    Task<Result> RemoverAsync(Guid id);
}