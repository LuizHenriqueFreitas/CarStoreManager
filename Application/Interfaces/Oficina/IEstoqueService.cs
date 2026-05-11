using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;

namespace CarStoreManager.Application.Interfaces;

public interface IEstoqueService
{
    Task<Result<IEnumerable<EstoqueComponenteDTO>>> ListarAsync();
    Task<Result<IEnumerable<EstoqueComponenteDTO>>> ListarComEstoqueBaixoAsync();
    Task<Result<EstoqueComponenteDTO>> ObterPorComponenteAsync(Guid componenteId);
    Task<Result> CriarOuAtualizarMinimoAsync(Guid componenteId, int quantidadeMinima);
    Task<Result> EntradaAsync(Guid componenteId, int quantidade);
    Task<Result> SaidaAsync(Guid componenteId, int quantidade);
}
