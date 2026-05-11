using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Oficina;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<Fornecedor?> ObterPorCnpjAsync(string cnpjNumeros);
}
