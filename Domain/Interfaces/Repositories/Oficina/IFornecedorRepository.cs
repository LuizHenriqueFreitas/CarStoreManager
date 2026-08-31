using CarStoreManager.Domain.Entities.Oficina;

namespace CarStoreManager.Domain.Repositories;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<bool> CnpjExisteAsync(string cnpj);

    /// <summary>Busca fornecedores ativos por nome/CNPJ (case-insensitive) — alimenta o autocomplete no cadastro de componente.</summary>
    Task<IEnumerable<Fornecedor>> BuscarAsync(string termo, int limite = 20);
}
