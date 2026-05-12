using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IOrdemServicoRepository : IRepository<OrdemServico>
{
    Task<OrdemServico?> ObterPorNumeroPublicoAsync(string numeroPublico);

    Task<IEnumerable<OrdemServico>> ObterPorMecanicoAsync(Guid mecanicoId);

    Task<IEnumerable<OrdemServico>> ObterPorClienteAsync(Guid clienteId);

    Task<IEnumerable<OrdemServico>> ObterPorStatusAsync(StatusOrdemServico status);

    /// <summary>
    /// Devolve todas as OS que têm pelo menos um item de Encomenda aguardando
    /// chegada de um determinado componente. Inclui os itens carregados.
    /// </summary>
    Task<IEnumerable<OrdemServico>> ObterComItensAguardandoAsync(Guid componenteId);

    /// <summary>
    /// Registra explicitamente um novo item no DbSet evitando ambiguidades de
    /// detecção de mudanças quando o pai já está tracked.
    /// </summary>
    Task AdicionarItemAsync(ItemOrdemServico item);

    /// <summary>
    /// Mesmo padrão do AdicionarItemAsync — explicitamente registra um novo
    /// item de checklist no DbSet para evitar o bug de concorrência onde o
    /// change tracker erra a inferência ao salvar via Update no pai.
    /// </summary>
    Task AdicionarItemChecklistAsync(ChecklistOrdemServico item);
}