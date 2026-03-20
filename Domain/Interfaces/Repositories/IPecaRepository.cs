using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Peca>> ObterTodosAsync();

    Task AdicionarAsync(Peca peca);

    Task AtualizarAsync(Peca peca);

    Task RemoverAsync(Guid id);
}