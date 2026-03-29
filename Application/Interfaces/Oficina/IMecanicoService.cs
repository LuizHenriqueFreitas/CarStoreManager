using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.Mecanico;

namespace CarStoreManager.Application.Interfaces;

public interface IMecanicoService : IService<
    MecanicoDTO,
    MecanicoListaDTO,
    CriarMecanicoDTO,
    AtualizarMecanicoDTO>
{
    // CONSULTAS
    Task<Result<IEnumerable<MecanicoLookupDTO>>> ObterDisponiveisAsync();

    // REGRAS DE NEGÓCIO
    Task<Result> AtualizarOcupacaoAsync(Guid mecanicoId);
}