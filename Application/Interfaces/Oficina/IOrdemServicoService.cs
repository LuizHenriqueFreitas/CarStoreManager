using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.Interfaces;

public interface IOrdemServicoService : IService<
    OrdemServicoDTO,
    OrdemServicoListaDTO,
    CriarOrdemServicoDTO,
    AtualizarOrdemServicoDTO>
{
    // CONSULTAS
    Task<Result<OrdemServicoPublicaDTO>> ObterPorNumeroPublicoAsync(string numeroPublico);

    // ITENS
    Task<Result> AdicionarItemAsync(AdicionarItemOrdemServicoDTO dto);
    Task<Result> RemoverItemAsync(Guid ordemId, Guid itemId);
    Task<Result> AtualizarItemAsync(AtualizarItemOrdemServicoDTO dto);

    // CHECKLIST
    Task<Result> AdicionarItemChecklistAsync(AdicionarChecklistItemDTO dto);
    Task<Result> RemoverItemChecklistAsync(Guid ordemId, Guid itemId);
    Task<Result> AtualizarDescricaoChecklistAsync(Guid ordemId, Guid itemId, string novaDescricao);
    Task<Result> AtualizarStatusChecklistAsync(AtualizarStatusChecklistDTO dto);

    // CÁLCULO
    Task<Result> RecalcularValoresAsync(Guid ordemId);

    // FLUXO DE APROVAÇÃO
    Task<Result> RegistrarAprovacaoDoClienteAsync(Guid ordemId);
    Task<Result> IniciarAsync(Guid ordemId);
    Task<Result> FinalizarAsync(Guid ordemId);

    // VISTORIA DE ENTRADA (contrato da OS, feito pelo recepcionista)
    Task<Result<VistoriaOrdemServicoDTO>> IniciarVistoriaAsync(Guid ordemId, Guid recepcionistaId);
    Task<Result<VistoriaOrdemServicoDTO>> ObterVistoriaAsync(Guid ordemId);
    Task<Result<VistoriaOrdemServicoDTO>> EditarVistoriaAsync(Guid ordemId, string novoTexto);
    Task<Result> ConcluirVistoriaAsync(Guid ordemId, string textoFinal);

    /// <summary>
    /// Recepção marca a OS como entregue ao cliente (após cobrança).
    /// Falha se a OS ainda não estiver totalmente paga.
    /// </summary>
    Task<Result> EntregarAsync(Guid ordemId);

    Task<Result> CancelarAsync(Guid ordemId);
}