using CarStoreManager.Domain.Base;
using System.Security.Cryptography;

namespace CarStoreManager.Domain.Entities.Integracoes;

/// <summary>
/// Singleton — existe apenas UM registro no BD, mirroring ConfiguracaoSistema.
/// Guarda o estado da conexão OAuth com o Mercado Livre. AccessToken/RefreshToken
/// chegam aqui já criptografados (via IDataProtector, na camada Application) —
/// esta entidade nunca vê o valor em texto puro.
/// </summary>
public class ConfiguracaoMercadoLivre : Entity
{
    public bool Conectado { get; private set; }
    public string? MercadoLivreUserId { get; private set; }
    public string? EmailContaConectada { get; private set; }
    public string? AccessTokenCriptografado { get; private set; }
    public string? RefreshTokenCriptografado { get; private set; }
    public DateTime? DataExpiracaoToken { get; private set; }
    public string? SegredoWebhook { get; private set; }
    public DateTime? DataConexao { get; private set; }

    protected ConfiguracaoMercadoLivre() { }

    /// <summary>Construtor para a primeira inicialização — admin conecta via OAuth antes do primeiro uso real.</summary>
    public ConfiguracaoMercadoLivre(bool _) : this() { }

    public void RegistrarConexao(
        string mercadoLivreUserId,
        string email,
        string accessTokenCriptografado,
        string refreshTokenCriptografado,
        DateTime dataExpiracaoToken)
    {
        MercadoLivreUserId = mercadoLivreUserId;
        EmailContaConectada = email;
        AccessTokenCriptografado = accessTokenCriptografado;
        RefreshTokenCriptografado = refreshTokenCriptografado;
        DataExpiracaoToken = dataExpiracaoToken;
        Conectado = true;
        DataConexao = DateTime.UtcNow;

        if (string.IsNullOrEmpty(SegredoWebhook))
            GerarSegredoWebhook();
    }

    public void AtualizarTokens(string accessTokenCriptografado, string refreshTokenCriptografado, DateTime dataExpiracaoToken)
    {
        AccessTokenCriptografado = accessTokenCriptografado;
        RefreshTokenCriptografado = refreshTokenCriptografado;
        DataExpiracaoToken = dataExpiracaoToken;
    }

    /// <summary>Gera um segredo aleatório (base64url) para validar o endpoint público do webhook — mesmo padrão de TermoEntrega.EnviarParaAssinatura().</summary>
    public void GerarSegredoWebhook()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        SegredoWebhook = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public void Desconectar()
    {
        Conectado = false;
        AccessTokenCriptografado = null;
        RefreshTokenCriptografado = null;
        DataExpiracaoToken = null;
    }

    public bool TokenExpirado()
        => !DataExpiracaoToken.HasValue || DateTime.UtcNow >= DataExpiracaoToken.Value.AddMinutes(-5);
}
