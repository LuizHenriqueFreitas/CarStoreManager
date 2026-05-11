using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.Interfaces;

/*
    Abstração para gravação física de arquivos.
    Implementação concreta vive em Infrastructure (acessa IWebHostEnvironment, FileSystem etc).
*/
public interface IArquivoStorage
{
    /// <summary>
    /// Persiste o arquivo abaixo da pasta lógica informada e devolve a URL pública relativa.
    /// </summary>
    Task<string> SalvarAsync(ArquivoUpload arquivo, string pastaLogica, CancellationToken ct = default);

    /// <summary>
    /// Remove o arquivo a partir da URL pública relativa devolvida por SalvarAsync.
    /// </summary>
    Task RemoverAsync(string urlPublica, CancellationToken ct = default);
}
