using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.VeiculoConsignacao;

namespace CarStoreManager.Application.Interfaces;

public interface IVeiculoConsignacaoService : IService<
    VeiculoConsignacaoDTO,
    VeiculoConsignacaoListaDTO,
    CriarVeiculoConsignacaoDTO,
    AtualizarVeiculoConsignacaoDTO>
{
    Task<Result<IEnumerable<VeiculoConsignacaoListaDTO>>> ObterAtivasAsync();
    Task<Result<IEnumerable<VeiculoConsignacaoListaDTO>>> ObterPorClienteProprietarioAsync(Guid clienteProprietarioId);
    Task<Result> RenovarAsync(Guid id, int diasAdicionais = 90);
    Task<Result> MarcarComoVendidaAsync(Guid id);
    Task<Result> ConcluirVendaAsync(Guid id);
    Task<Result> DevolverAsync(Guid id);
    Task<Result> CancelarAsync(Guid id, string motivo);
}
