using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace Domain.Interfaces;

public interface IVendedorRepository : IRepository<Vendedor>
{
    Task<Vendedor?> GetByCpfAsync(string cpf);
}