using CarStoreManager.Domain.Entities.Concessionaria;

namespace CarStoreManager.Domain.Repositories;

public interface IVendedorRepository : IRepository<Vendedor>
{
    Task<Vendedor?> GetByCpfAsync(string cpf);
}