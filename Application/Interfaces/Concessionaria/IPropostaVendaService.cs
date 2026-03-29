using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

namespace CarStoreManager.Application.Interfaces;

public interface IPropostaVendaService : IService<
    PropostaVendaDTO,
    PropostaVendaListaDTO,
    CriarPropostaVendaDTO,
    PropostaVendaDTO>//esse ultimo parametro nao sera utilizado pois esta classe nao faz update - ele muda o status direto no server por meio de funções internas da classe
{
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorVendedorAsync(Guid vendedorId);
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorClienteAsync(Guid clienteId);
    Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto);
    Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto);
    Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto);
    Task<Result> AprovarAsync(Guid propostaId);
    Task<Result> RejeitarAsync(Guid propostaId);
    Task<Result> CancelarAsync(Guid propostaId);
}