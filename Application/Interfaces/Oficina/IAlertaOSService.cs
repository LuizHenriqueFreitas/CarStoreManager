using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.Interfaces;

public interface IAlertaOSService
{
    Task<Result<AlertaOSDTO>> EmitirAsync(Guid ordemId, Guid mecanicoId, CriarAlertaOSDTO dto);
    Task<Result<IEnumerable<AlertaOSDTO>>> ListarPendentesAsync();
    Task<Result<IEnumerable<AlertaOSDTO>>> ListarPorOrdemAsync(Guid ordemId);

    /// <summary>
    /// Recepcionista registra a decisão do cliente. Em ambos os casos a OS
    /// volta para EmAndamento — a diferença está no histórico do alerta.
    /// </summary>
    Task<Result> ResolverAsync(Guid alertaId, Guid resolvidoPor, ResolverAlertaDTO dto);
}
