using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Oficina.OrdemServico;

namespace CarStoreManager.Application.Interfaces;

public interface IOrdemServicoService
{
    // CONSULTAS
    Task<Result<OrdemServicoDTO>> ObterPorIdAsync(Guid id);
    Task<Result<IEnumerable<OrdemServicoListaDTO>>> ObterTodasAsync();
    Task<Result<OrdemServicoPublicaDTO>> ObterPorNumeroPublicoAsync(string numeroPublico);

    // CRIAÇÃO
    Task<Result<Guid>> CriarAsync(CriarOrdemServicoDTO dto);

    // ITENS
    Task<Result> AdicionarItemAsync(AdicionarItemOrdemServicoDTO dto);
    Task<Result> RemoverItemAsync(Guid ordemId, Guid itemId);
    Task<Result> AtualizarItemAsync(AtualizarItemOrdemServicoDTO dto);

    // STATUS
    Task<Result> AtualizarStatusAsync(AtualizarOrdemServicoDTO dto);

    // CHECKLIST
    Task<Result> AdicionarItemChecklistAsync(AdicionarChecklistItemDTO dto);
    Task<Result> AtualizarStatusChecklistAsync(AtualizarStatusChecklistDTO dto);

    // CÁLCULO
    Task<Result> RecalcularValoresAsync(Guid ordemId);
}