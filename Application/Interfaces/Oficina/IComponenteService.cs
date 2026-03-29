using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;

namespace CarStoreManager.Application.Interfaces;

public interface IComponenteService : IService<
    ComponenteDTO,
    ComponenteListaDTO,
    CriarComponenteDTO,
    AtualizarComponenteDTO>
{
    // CONSULTAS
    Task<Result<IEnumerable<ComponenteListaDTO>>> ObterComEstoqueBaixoAsync();
    Task<Result<IEnumerable<ComponenteLookupDTO>>> ObterPorSistemaAsync(string sistema);

    // ESTOQUE (REGRA DE NEGÓCIO)
    Task<Result> EntradaEstoqueAsync(Guid componenteId, int quantidade);
    Task<Result> SaidaEstoqueAsync(Guid componenteId, int quantidade);
}