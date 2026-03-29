using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IOrdemServicoRepository : IRepository<OrdemServico>
{
    Task<OrdemServico?> ObterPorNumeroPublicoAsync(string numeroPublico);

    Task<IEnumerable<OrdemServico>> ObterPorMecanicoAsync(Guid mecanicoId);

    Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId);

    Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status);
}