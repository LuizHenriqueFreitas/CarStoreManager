using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface IConfiguracaoSistemaService
{
    Task<Result<ConfiguracaoSistemaDTO>> ObterAsync();
    Task<Result> AtualizarAsync(ConfiguracaoSistemaDTO dto);
    Task<Result> TestarEnvioAsync(string emailDestino);
}
