using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoClienteRepository : IRepository<VeiculoCliente>
{
    Task<IEnumerable<VeiculoCliente>> ObterPorClienteAsync(Guid clienteId);
}