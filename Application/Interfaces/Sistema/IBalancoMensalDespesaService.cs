using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;

namespace CarStoreManager.Application.Interfaces.Sistema;

public interface IBalancoMensalDespesaService
{
    /// <summary>Balanço da competência (cria em memória se ainda não existe).</summary>
    Task<Result<BalancoMensalDespesaDTO>> ObterAsync(int ano, int mes);

    /// <summary>Lista compacta (ano/mês/fechado/total) de todos os balanços salvos.</summary>
    Task<Result<IEnumerable<BalancoMensalDespesaDTO>>> HistoricoAsync();

    /// <summary>Cria o balanço da competência a partir do formulário-modelo.</summary>
    Task<Result<BalancoMensalDespesaDTO>> GerarDoModeloAsync(int ano, int mes);

    Task<Result> SalvarItemAsync(SalvarItemBalancoDTO dto);
    Task<Result> RemoverItemAsync(int ano, int mes, Guid itemId);
    Task<Result> FecharAsync(int ano, int mes);
    Task<Result> ReabrirAsync(int ano, int mes);

    /// <summary>Total efetivo de despesas do mês (balanço se existir; senão soma do modelo ativo).</summary>
    Task<Result<decimal>> TotalDoMesAsync(int ano, int mes);

    /// <summary>Dias restantes até o próximo fechamento; null se > 5 dias (sem aviso).</summary>
    Task<int?> DiasParaFechamentoAsync();
}
