using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Interfaces.Repositories.Integracoes;

namespace CarStoreManager.Application.Services.Integracoes;

/// <summary>
/// Garante um access token válido antes de qualquer chamada à API do ML,
/// renovando via refresh token quando necessário. Compartilhado entre
/// MercadoLivrePublicacaoService e MercadoLivreSincronizacaoService — registrado
/// como classe concreta na DI, mesmo padrão usado por CsvReportFormatter/XmlReportFormatter.
/// </summary>
public class MercadoLivreTokenHelper
{
    private readonly IConfiguracaoMercadoLivreRepository _configRepo;
    private readonly ITokenCriptografiaService _cripto;
    private readonly IMercadoLivreApiClient _apiClient;

    public MercadoLivreTokenHelper(
        IConfiguracaoMercadoLivreRepository configRepo,
        ITokenCriptografiaService cripto,
        IMercadoLivreApiClient apiClient)
    {
        _configRepo = configRepo;
        _cripto = cripto;
        _apiClient = apiClient;
    }

    public async Task<Result<string>> ObterAccessTokenValidoAsync()
    {
        var cfg = await _configRepo.ObterAsync();
        if (!cfg.Conectado || string.IsNullOrEmpty(cfg.AccessTokenCriptografado))
            return Result<string>.Fail("Mercado Livre não conectado. Acesse Integrações > Mercado Livre para conectar.");

        if (cfg.TokenExpirado())
        {
            if (string.IsNullOrEmpty(cfg.RefreshTokenCriptografado))
                return Result<string>.Fail("Token expirado e sem refresh token disponível. Reconecte o Mercado Livre.");

            var refreshToken = _cripto.Desproteger(cfg.RefreshTokenCriptografado);
            var novoToken = await _apiClient.RenovarTokenAsync(refreshToken);

            cfg.AtualizarTokens(
                _cripto.Proteger(novoToken.AccessToken),
                _cripto.Proteger(novoToken.RefreshToken),
                DateTime.UtcNow.AddSeconds(novoToken.ExpiresIn));
            await _configRepo.SaveChangesAsync();

            return Result<string>.Ok(novoToken.AccessToken);
        }

        return Result<string>.Ok(_cripto.Desproteger(cfg.AccessTokenCriptografado));
    }
}
