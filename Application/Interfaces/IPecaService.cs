using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.Interfaces;

public interface IPecaService
{
    //GET /id
    Task<Result<PecaDTO>> ObterPorIdAsync(Guid id);

    //GET
    Task<Result<IEnumerable<PecaDTO>>> ObterTodosAsync();

    //POST
    Task<Result> CriarAsync(PecaDTO produto);
    
    //PUT
    Task<Result> AtualizarAsync(PecaDTO produto);
    
    //DELETE
    Task<Result> RemoverAsync(Guid id);
}