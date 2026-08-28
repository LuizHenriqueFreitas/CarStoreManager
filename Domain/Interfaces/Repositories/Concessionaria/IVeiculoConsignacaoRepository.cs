using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Repositories;

public interface IVeiculoConsignacaoRepository : IRepository<VeiculoConsignacao>
{
    Task<IEnumerable<VeiculoConsignacao>> ObterPorStatusAsync(StatusConsignacao status);
    Task<IEnumerable<VeiculoConsignacao>> ObterPorClienteAsync(Guid clienteProprietarioId);
    Task<IEnumerable<VeiculoConsignacao>> ObterPorVendedorAsync(Guid vendedorResponsavelId);
}
