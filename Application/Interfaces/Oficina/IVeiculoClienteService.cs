using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.VeiculoCliente;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoClienteService :IService<
    VeiculoClienteDTO,
    VeiculoClienteListaDTO,
    CriarVeiculoClienteDTO,
    AtualizarVeiculoClienteDTO>
{
    Task<Result<IEnumerable<VeiculoClienteListaDTO>>> ObterPorClienteAsync(Guid clienteId);
}