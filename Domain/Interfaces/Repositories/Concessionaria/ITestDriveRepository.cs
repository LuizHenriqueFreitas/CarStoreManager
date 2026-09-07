using CarStoreManager.Domain.Entities.Concessionaria;

namespace CarStoreManager.Domain.Repositories;

public interface ITestDriveRepository : IRepository<TestDrive>
{
    Task<IEnumerable<TestDrive>> ObterPorVeiculoAsync(Guid veiculoVendaId);
    Task<IEnumerable<TestDrive>> ObterPorClienteAsync(Guid clienteId);
}
