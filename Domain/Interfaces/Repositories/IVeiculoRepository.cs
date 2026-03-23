using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId);
}