using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Domain.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Cliente>> ObterTodosAsync();

    Task AdicionarAsync(Cliente cliente);

    Task AtualizarAsync(Cliente cliente);

    Task RemoverAsync(Guid id);
}