using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace Domain.Interfaces;

public interface IVeiculoRepository : IRepository<Veiculo>
{
    Task<Veiculo?> GetByPlacaAsync(string placa);

    Task<IEnumerable<Veiculo>> GetDisponiveisAsync();
}