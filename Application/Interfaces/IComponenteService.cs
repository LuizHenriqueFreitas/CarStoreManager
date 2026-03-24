using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Componente;

namespace CarStoreManager.Application.Interfaces;

public interface IComponenteService
{
    // =========================
    // CONSULTAS
    // =========================

    Task<Result<ComponenteDTO>> ObterPorIdAsync(Guid id);

    Task<Result<IEnumerable<ComponenteListaDTO>>> ObterTodosAsync();


    // =========================
    // CRUD
    // =========================

    Task<Result<Guid>> CriarAsync(CriarComponenteDTO dto);

    Task<Result> AtualizarAsync(AtualizarComponenteDTO dto);

    Task<Result> RemoverAsync(Guid id);


    // =========================
    // ESTOQUE (REGRA DE NEGÓCIO)
    // =========================

    Task<Result> EntradaEstoqueAsync(Guid componenteId, int quantidade);

    Task<Result> SaidaEstoqueAsync(Guid componenteId, int quantidade);
}