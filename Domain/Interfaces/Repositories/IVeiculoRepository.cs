using CarStoreManager.Domain.Entities;
using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId);

    Task AdicionarAsync(Veiculo veiculo);

    Task AtualizarAsync(Veiculo veiculo);

    Task RemoverAsync(Guid id);
}