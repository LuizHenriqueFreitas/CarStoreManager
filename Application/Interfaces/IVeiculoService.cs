using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Veiculo;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoService
{
    Task<Result<IEnumerable<VeiculoListaDTO>>> ObterTodosAsync ();

    Task<Result<VeiculoDTO>> ObterPorIdAsync(Guid id);
    
    Task<Result> CriarAsync(CriarVeiculoDTO veiculo);
    Task<Result> AtualizarAsync(AtualizarVeiculoDTO veiculo);
    Task<Result> RemoverAsync(Guid id);

    Task<Result> MarcarComoVendidoAsync(Guid id);
    Task<Result> MarcarComoDisponivelAsync(Guid id);
    Task<Result> AtualizarQuilometragemAsync(Guid id, int km);
}