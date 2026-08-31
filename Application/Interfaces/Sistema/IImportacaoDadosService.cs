using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema.Importacao;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface IImportacaoDadosService
{
    Task<Result<ImportacaoResultadoDTO>> ImportarAsync(ImportacaoDadosDTO dados);
}
