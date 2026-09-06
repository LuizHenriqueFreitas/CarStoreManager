namespace CarStoreManager.Application.Common;

/// <summary>
/// Erro devolvido pela API do Mercado Livre, já traduzido para uma mensagem
/// amigável em português (ver MercadoLivreApiClientReal.TraduzirErro), mas
/// preservando o corpo técnico original — quem for apresentar o sistema
/// precisa poder mostrar a resposta crua se perguntarem (ver tela de anúncios,
/// detalhe expansível em cima de AnuncioMercadoLivre.UltimoErroDetalheTecnico).
/// </summary>
public class MercadoLivreApiException : Exception
{
    public string DetalheTecnico { get; }

    public MercadoLivreApiException(string mensagemAmigavel, string detalheTecnico)
        : base(mensagemAmigavel)
    {
        DetalheTecnico = detalheTecnico;
    }
}
