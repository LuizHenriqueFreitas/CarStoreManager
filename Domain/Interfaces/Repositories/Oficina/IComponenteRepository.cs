using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IComponenteRepository : IRepository<Componente>
{
    Task<IEnumerable<Componente>> ObterComEstoqueBaixoAsync();
    Task<IEnumerable<Componente>> ObterPorSistemaAsync(SistemaComponente sistema);

    /// <summary>
    /// Busca componentes ativos por substring em Nome, SKUInterno, PartNumber,
    /// CodigoOEM ou CodigoBarras (case-insensitive). Usado pelo autocomplete
    /// na criação/edição de OS.
    /// </summary>
    Task<IEnumerable<Componente>> BuscarAsync(string termo, int limite = 20);
}