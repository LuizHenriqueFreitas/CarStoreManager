using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Domain.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> ObterPorCpfAsync(string cpf);
    Task<bool> CpfExisteAsync(string cpf);
    Task<List<Cliente>> PesquisarAsync(string termo);
}