using SistemaEmpresa.Domain.Entities.Oficina;

namespace Domain.Interfaces;

public interface IOrdemServicoRepository : IRepository<OrdemServico>
{
    Task<IEnumerable<OrdemServico>> GetByMecanicoAsync(Guid mecanicoId);

    Task<IEnumerable<OrdemServico>> GetEmAbertoAsync();

    Task<IEnumerable<OrdemServico>> GetConcluidasAsync();
}