using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;

namespace CarStoreManager.Application.Interfaces;

public interface IMercadoLivreSincronizacaoService
{
    Task<Result<string>> ObterUrlConexaoAsync(string state);
    Task<Result> ProcessarCallbackOAuthAsync(string code);
    Task<Result> DesconectarAsync();
    Task<Result<ConfiguracaoMercadoLivreDTO>> ObterConfiguracaoAsync();

    /// <summary>Chamado pelo endpoint de webhook — sempre idempotente, nunca lança.</summary>
    Task<Result> ProcessarNotificacaoWebhookAsync(MercadoLivreWebhookNotificacaoDTO notificacao);
}
