using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema.Documentos;

namespace CarStoreManager.Application.Interfaces.Sistema;

/// <summary>
/// Central de Documentos — listagem enriquecida (cliente/responsável/placa)
/// dos documentos já produzidos pelo sistema, para busca e exportação em PDF.
/// Só leitura: a edição de cada documento continua na tela de origem
/// (proposta, test drive, veículo consignado, ordem de serviço).
/// </summary>
public interface IDocumentosService
{
    Task<Result<List<DocumentoDTO>>> ListarTermosEntregaAsync();
    Task<Result<List<DocumentoDTO>>> ListarTermosTestDriveAsync();
    Task<Result<List<DocumentoDTO>>> ListarContratosConsignacaoAsync();
    Task<Result<List<DocumentoDTO>>> ListarContratosOSAsync();
}
