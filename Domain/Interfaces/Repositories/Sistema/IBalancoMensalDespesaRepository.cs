using CarStoreManager.Domain.Entities.Sistema;

namespace CarStoreManager.Domain.Interfaces.Repositories.Sistema;

public interface IBalancoMensalDespesaRepository
{
    Task<BalancoMensalDespesa?> ObterPorCompetenciaAsync(DateOnly competencia);
    Task<IEnumerable<BalancoMensalDespesa>> ListarAsync();
    Task AddAsync(BalancoMensalDespesa balanco);
    void Update(BalancoMensalDespesa balanco);
    Task SaveChangesAsync();
}
