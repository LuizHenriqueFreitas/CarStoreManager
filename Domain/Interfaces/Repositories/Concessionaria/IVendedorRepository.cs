using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IVendedorRepository : IRepository<Vendedor>
{
    Task<IEnumerable<Vendedor>> ObterPorNivelAsync(NivelFuncionario nivel);
}