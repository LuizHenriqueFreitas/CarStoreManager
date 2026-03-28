using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoVenda;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoVendaService
{
    Task<Result<VeiculoVendaDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<VeiculoVendaListaDTO>>> ObterTodosAsync();
    Task<Result<IEnumerable<VeiculoVendaListaDTO>>> ObterDisponiveisAsync();
    Task<Result<Guid>> CriarAsync(CriarVeiculoVendaDTO dto);
    Task<Result> AtualizarAsync(AtualizarVeiculoVendaDTO dto);
    Task<Result> RemoverAsync(Guid id);
    Task<Result> MarcarComoVendidoAsync(Guid id);
    Task<Result> MarcarComoDisponivelAsync(Guid id);
    Task<Result> AtualizarQuilometragemAsync(Guid id, int km);
    Task<Result> AdicionarFotoAsync(Guid id, string url);
    Task<Result> RemoverFotoAsync(Guid id, Guid fotoId);
}