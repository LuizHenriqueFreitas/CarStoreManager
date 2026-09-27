using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IComponenteEquivalenteRepository : IRepository<ComponenteEquivalente>
{
    /// <summary>
    /// Vínculos curados que envolvem o componente, nas duas direções da FK
    /// (o componente pode estar como Original ou como Equivalente).
    /// </summary>
    Task<IEnumerable<ComponenteEquivalente>> ObterPorComponenteAsync(Guid componenteId);

    /// <summary>
    /// Existe vínculo entre os dois componentes, em qualquer direção?
    /// Usado pra checagem de duplicidade antes de criar um novo vínculo.
    /// </summary>
    Task<ComponenteEquivalente?> ObterLigacaoEntreAsync(Guid componenteAId, Guid componenteBId);
}
