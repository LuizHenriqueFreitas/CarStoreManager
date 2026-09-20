using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

namespace CarStoreManager.Application.Interfaces;

public interface IVendedorService : IService<
    VendedorDTO,
    VendedorListaDTO,
    CriarVendedorDTO,
    AtualizarVendedorDTO>
{
    Task<Result<List<VendedorListaDTO>>> PesquisarAsync(string termo);
}