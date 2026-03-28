using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

namespace CarStoreManager.Application.Interfaces;

public interface IVendedorService
{
    Task<Result<VendedorDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<VendedorListaDTO>>> ObterTodosAsync();
    Task<Result<Guid>> CriarAsync(CriarVendedorDTO dto);
    Task<Result> AtualizarAsync(AtualizarVendedorDTO dto);
    Task<Result> RemoverAsync(Guid id);
}