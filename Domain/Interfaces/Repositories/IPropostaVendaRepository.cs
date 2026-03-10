using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace Domain.Interfaces;

public interface IPropostaVendaRepository : IRepository<PropostaVenda>
{
    Task<IEnumerable<PropostaVenda>> GetByVeiculoAsync(Guid veiculoId);

    Task<IEnumerable<PropostaVenda>> GetByVendedorAsync(Guid vendedorId);

    Task<IEnumerable<PropostaVenda>> GetPendentesAsync();
}