using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.Interfaces;

/// <summary>
/// Consulta pública de ordens de serviço pelo cliente final — autenticada por
/// CPF + placa de um dos veículos dele. Ver docs/redesign/08-consulta-cliente-cpf.md.
/// </summary>
public interface IConsultaPublicaService
{
    Task<Result<ConsultaPublicaResultadoDTO>> ConsultarPorCpfEPlacaAsync(string cpf, string placa);
}
