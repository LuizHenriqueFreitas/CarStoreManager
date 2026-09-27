using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IVistoriaOrdemServicoRepository : IRepository<VistoriaOrdemServico>
{
    Task<VistoriaOrdemServico?> ObterPorOrdemServicoAsync(Guid ordemServicoId);
}
