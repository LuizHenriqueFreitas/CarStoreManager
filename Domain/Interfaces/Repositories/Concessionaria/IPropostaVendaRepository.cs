using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IPropostaVendaRepository : IRepository<PropostaVenda>
{
    Task<IEnumerable<PropostaVenda>> ObterPorVendedorAsync(Guid vendedorId);
    Task<IEnumerable<PropostaVenda>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<PropostaVenda>> ObterPorStatusAsync(StatusPropostaVenda status);
}