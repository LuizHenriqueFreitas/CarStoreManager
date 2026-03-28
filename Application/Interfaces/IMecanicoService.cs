using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;

namespace CarStoreManager.Application.Interfaces;

public interface IMecanicoService
{
    // =========================
    // CONSULTAS
    // =========================

    Task<Result<MecanicoDTO>> ObterPorIdAsync(Guid id);

    Task<Result<IEnumerable<MecanicoListaDTO>>> ObterTodosAsync();

    Task<Result<IEnumerable<MecanicoLookupDTO>>> ObterDisponiveisAsync();


    // =========================
    // CRUD
    // =========================

    Task<Result<Guid>> CriarAsync(CriarMecanicoDTO dto);

    Task<Result> AtualizarAsync(AtualizarMecanicoDTO dto);

    Task<Result> RemoverAsync(Guid id);


    // =========================
    // REGRAS DE NEGÓCIO
    // =========================

    Task<Result> AtualizarOcupacaoAsync(Guid mecanicoId);
}