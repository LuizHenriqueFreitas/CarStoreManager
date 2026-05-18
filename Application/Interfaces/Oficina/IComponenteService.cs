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
    Task<Result<IEnumerable<ComponenteDTO>>> ObterComEstoqueBaixoAsync();
    Task<Result<IEnumerable<ComponenteDTO>>> ObterPorSistemaAsync(string sistema);

    /// <summary>
    /// Busca componentes por substring em Nome/SKU/PartNumber/OEM/CodigoBarras.
    /// Usado pelo autocomplete na criação e edição de OS.
    /// </summary>
    Task<Result<IEnumerable<ComponenteBuscaDTO>>> BuscarAsync(string termo, int limite = 20);

    // ESTOQUE (REGRA DE NEGÓCIO)
    Task<Result> EntradaEstoqueAsync(Guid componenteId, int quantidade);
    Task<Result> SaidaEstoqueAsync(Guid componenteId, int quantidade);

    // EQUIVALÊNCIA — busca componentes que servem como substituto.
    // Critérios: mesmo CodigoOEM (cross-brand) OU registrados como ComponenteEquivalente.
    Task<Result<List<ComponenteListaDTO>>> ObterEquivalentesAsync(Guid componenteId);

    // PRECIFICAÇÃO — sobrescreve margem individual e recalcula valor de venda.
    Task<Result> AjustarMargemAsync(Guid id, decimal novaMargemPct);
}
