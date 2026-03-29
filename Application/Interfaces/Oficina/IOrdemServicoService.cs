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
    Task<Result> AtualizarStatusChecklistAsync(AtualizarStatusChecklistDTO dto);

    // CÁLCULO
    Task<Result> RecalcularValoresAsync(Guid ordemId);
}