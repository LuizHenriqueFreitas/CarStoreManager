using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs;

namespace CarStoreManager.Application.Interfaces;

public interface IFotoService
{
    /// <summary>
    /// Upload de uma ou mais fotos para uma entidade.
    /// </summary>
    Task<Result<List<FotoDto>>> UploadFotosAsync(string entidadeTipo, Guid entidadeId, IList<ArquivoUpload> arquivos);

    /// <summary>
    /// Remove uma foto específica.
    /// </summary>
    Task<Result> RemoverFotoAsync(Guid fotoId);

    /// <summary>
    /// Reordena as fotos de uma entidade (lista de IDs na nova ordem).
    /// </summary>
    Task<Result> ReordenarFotosAsync(string entidadeTipo, Guid entidadeId, List<Guid> ordemIds);

    /// <summary>
    /// Obtém todas as fotos de uma entidade com seus metadados.
    /// </summary>
    Task<Result<List<FotoDto>>> GetFotosByEntidadeAsync(string entidadeTipo, Guid entidadeId);
}
