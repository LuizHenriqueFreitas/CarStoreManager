using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoRepository : IRepository<VeiculoVenda>
{
    Task<IEnumerable<VeiculoVenda>> ObterPorClienteAsync(Guid clienteId);
}