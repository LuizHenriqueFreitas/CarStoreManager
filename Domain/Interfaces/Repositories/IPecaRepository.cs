using SistemaEmpresa.Domain.Entities.Oficina;

namespace Domain.Interfaces;

public interface IPecaRepository : IRepository<Peca>
{
    Task<Peca?> GetByModeloAsync(string modelo);

    Task<IEnumerable<Peca>> GetDisponiveisAsync();
}