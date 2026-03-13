using CarStoreManager.Domain.Entities;
using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace CarStoreManager.Domain.Repositories;

public interface IPropostaVendaRepository
{
    Task<IEnumerable<PropostaVenda>> ObterTodasAsync();

    Task<PropostaVenda?> ObterPorIdAsync(Guid id);

    Task AdicionarAsync(PropostaVenda proposta);

    Task AtualizarAsync(PropostaVenda proposta);

    Task RemoverAsync(Guid id);
}