using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Concessionaria.PropostaVenda;

namespace CarStoreManager.Application.Interfaces;

public interface IPropostaVendaService : IService<
    PropostaVendaDTO,
    PropostaVendaListaDTO,
    CriarPropostaVendaDTO,
    PropostaVendaDTO>
{
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorVendedorAsync(Guid vendedorId);
    Task<Result<IEnumerable<PropostaVendaListaDTO>>> ObterPorClienteAsync(Guid clienteId);

    Task<Result> AplicarDescontoAsync(AplicarDescontoDTO dto);
    Task<Result> DefinirEntradaAsync(DefinirEntradaDTO dto);
    Task<Result> DefinirModoPagamentoAsync(Guid propostaId, string modoPagamento);

    /// <summary>
    /// Marca proposta como AguardandoFinanciadora E envia e-mail com dados
    /// (cliente, veículo, entrada, valor líquido) para a financiadora
    /// configurada em ConfiguracaoSistema.
    /// </summary>
    Task<Result> SolicitarFinanciamentoAsync(Guid propostaId);

    Task<Result> RegistrarRespostaFinanciadoraAsync(Guid propostaId, RegistrarRespostaFinanciadoraDTO dto);

    Task<Result> AprovarAsync(Guid propostaId);
    Task<Result> RejeitarAsync(Guid propostaId, string motivo);
    Task<Result> CancelarAsync(Guid propostaId, string motivo);

    // === Vistoria ===
    Task<Result> IniciarVistoriaAsync(Guid propostaId);
    Task<Result<VistoriaDTO>> RegistrarVistoriaAsync(Guid propostaId, Guid vistoriadorId, RegistrarVistoriaDTO dto);
    Task<Result<IEnumerable<VistoriaDTO>>> ListarVistoriasAsync(Guid propostaId);

    // === Termo de entrega ===
    Task<Result<TermoEntregaDTO>> CriarOuEditarTermoAsync(Guid propostaId, Guid adminId, CriarOuEditarTermoDTO dto);
    Task<Result<TermoEntregaDTO>> ObterTermoAsync(Guid propostaId);
    Task<Result> EnviarTermoParaAssinaturaAsync(Guid propostaId);
    Task<Result<TermoEntregaDTO>> ObterTermoPorTokenAsync(string token);
    Task<Result> AssinarTermoAsync(string token, AssinarTermoDTO dto, string ipOrigem);

    // Compat com chamadores antigos (sem motivo) — depreciado.
    Task<Result> GerarFinanciamentoAsync(GerarFinanciamentoDTO dto);
}
