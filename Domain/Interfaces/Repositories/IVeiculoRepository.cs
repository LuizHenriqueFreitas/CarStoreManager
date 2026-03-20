using CarStoreManager.Domain.Entities;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoRepository
{
    Task<IEnumerable<Veiculo>> ObterTodosAsync();

    Task<Veiculo?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId);

    Task AdicionarAsync(Veiculo veiculo);

    Task AtualizarAsync(Veiculo veiculo);

    Task RemoverAsync(Guid id);
}