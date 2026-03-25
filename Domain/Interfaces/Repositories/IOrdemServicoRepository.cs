using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IOrdemServicoRepository : IRepository<OrdemServico>
{
    Task<OrdemServico?> ObterPorNumeroPublicoAsync(string numeroPublico);
}