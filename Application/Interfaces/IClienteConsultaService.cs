using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Shared.Cliente;

namespace CarStoreManager.Application.Interfaces;

/// <summary>
/// Consultas agregadas de cliente (ficha 360°) — separado de IClienteService
/// para não inflar a dependência do CRUD básico.
/// </summary>
public interface IClienteConsultaService
{
    Task<Result<ClienteFichaDTO>> ObterFichaAsync(Guid clienteId);
}
