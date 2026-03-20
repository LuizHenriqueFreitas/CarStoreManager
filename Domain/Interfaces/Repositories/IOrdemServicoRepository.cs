using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IOrdemServicoRepository
{
    Task<IEnumerable<OrdemServico>> ObterTodasAsync();

    Task<OrdemServico?> ObterPorIdAsync(Guid id);

    Task AdicionarAsync(OrdemServico ordemServico);

    Task AtualizarAsync(OrdemServico ordemServico);

    Task RemoverAsync(Guid id);
}