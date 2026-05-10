using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;

namespace CarStoreManager.Application.Interfaces;

public interface IComponenteService : IService<
    ComponenteDTO,
    ComponenteDTO,
    ComponenteDTO,
    ComponenteDTO>
{
    // CONSULTAS
    Task<Result<IEnumerable<ComponenteDTO>>> ObterComEstoqueBaixoAsync();
    Task<Result<IEnumerable<ComponenteDTO>>> ObterPorSistemaAsync(string sistema);

    // ESTOQUE (REGRA DE NEGÓCIO)
    Task<Result> EntradaEstoqueAsync(Guid componenteId, int quantidade);
    Task<Result> SaidaEstoqueAsync(Guid componenteId, int quantidade);
}