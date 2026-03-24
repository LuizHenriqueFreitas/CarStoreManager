using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.DTOs.Concessionaria.Vendedor;

public interface IVendedorService
{
    Task<Result<VendedorDTO>> ObterPorIdAsync(Guid id);

    Task<Result<IEnumerable<VendedorListaDTO>>> ObterTodosAsync();

    Task<Result<Guid>> CriarAsync(CriarVendedorDTO dto);

    Task<Result> AtualizarAsync(AtualizarVendedorDto dto);

    Task<Result> RemoverAsync(Guid id);
}