using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;

public interface ITermoTestDriveRepository : IRepository<TermoTestDrive>
{
    Task<TermoTestDrive?> ObterPorTestDriveAsync(Guid testDriveId);
    Task<TermoTestDrive?> ObterPorTokenAsync(string token);
}
