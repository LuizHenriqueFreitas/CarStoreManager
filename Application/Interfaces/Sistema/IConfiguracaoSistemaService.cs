using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface IConfiguracaoSistemaService
{
    Task<Result<ConfiguracaoSistemaDTO>> ObterAsync();
    Task<Result> AtualizarAsync(ConfiguracaoSistemaDTO dto);

    Task<Result<MargensDTO>> ObterMargensAsync();
    Task<Result> AtualizarMargensAsync(MargensDTO dto);

    /// <summary>Ponto único de consulta pra todo o resto do sistema saber quais módulos estão ligados.</summary>
    Task<Result<(bool Concessionaria, bool Oficina)>> ObterModulosAtivosAsync();
    Task<Result> AtualizarModulosAsync(bool concessionariaAtivo, bool oficinaAtivo);
}
