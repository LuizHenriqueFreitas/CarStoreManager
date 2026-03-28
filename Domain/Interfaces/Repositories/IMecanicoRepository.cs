using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IMecanicoRepository : IRepository<Mecanico>
{
    Task<IEnumerable<Mecanico>> ObterPorEspecialidadeAsync(EspecialidadeMecanico especialidade);
    Task<IEnumerable<Mecanico>> ObterPorNivelAsync(NivelFuncionario nivel);
}