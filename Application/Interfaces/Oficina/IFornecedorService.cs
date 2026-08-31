using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Fornecedor;

namespace CarStoreManager.Application.Interfaces;

public interface IFornecedorService : IService<
    FornecedorDTO,
    FornecedorListaDTO,
    CriarFornecedorDTO,
    AtualizarFornecedorDTO>
{
    Task<Result> AtivarAsync(Guid id);
    Task<Result> DesativarAsync(Guid id);

    /// <summary>Busca fornecedores ativos por nome/CNPJ — alimenta o campo de pesquisa no cadastro de componente.</summary>
    Task<Result<IEnumerable<FornecedorBuscaDTO>>> BuscarAsync(string termo, int limite = 20);
}
