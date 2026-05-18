using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Repositories;

namespace CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;

public interface IVistoriaRepository : IRepository<Vistoria>
{
    Task<IEnumerable<Vistoria>> ObterPorPropostaAsync(Guid propostaId);

    /// <summary>
    /// Retorna a vistoria em andamento (Concluida=false) da proposta, se houver.
    /// Usado para anexar fotos e depois registrar resultado.
    /// </summary>
    Task<Vistoria?> ObterPendentePorPropostaAsync(Guid propostaId);
}
