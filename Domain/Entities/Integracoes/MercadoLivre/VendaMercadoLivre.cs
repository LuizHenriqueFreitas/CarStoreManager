using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Enums;

namespace CarStoreManager.Domain.Entities.Integracoes;

/// <summary>
/// Registro de uma venda notificada pelo Mercado Livre. É a chave de
/// idempotência (IdPedidoPlataforma + IdItemPlataforma) — antes de processar
/// qualquer notificação, o service verifica se já existe um registro para o
/// mesmo par, evitando decrementar estoque duas vezes numa entrega duplicada
/// de webhook (comportamento "at-least-once" é padrão em webhooks reais).
/// </summary>
public class VendaMercadoLivre : Entity
{
    public Guid AnuncioMercadoLivreId { get; private set; }
    public string IdPedidoPlataforma { get; private set; } = null!;
    public string IdItemPlataforma { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public DateTime DataVenda { get; private set; }
    public StatusSincronizacaoVendaMercadoLivre StatusSincronizacao { get; private set; }
    public string? ErroSincronizacao { get; private set; }

    protected VendaMercadoLivre() { }

    public VendaMercadoLivre(
        Guid anuncioMercadoLivreId,
        string idPedidoPlataforma,
        string idItemPlataforma,
        int quantidade,
        decimal precoUnitario)
    {
        if (string.IsNullOrWhiteSpace(idPedidoPlataforma))
            throw new ArgumentException("Id do pedido é obrigatório.", nameof(idPedidoPlataforma));
        if (string.IsNullOrWhiteSpace(idItemPlataforma))
            throw new ArgumentException("Id do item é obrigatório.", nameof(idItemPlataforma));
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que 0.", nameof(quantidade));

        AnuncioMercadoLivreId = anuncioMercadoLivreId;
        IdPedidoPlataforma = idPedidoPlataforma;
        IdItemPlataforma = idItemPlataforma;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        DataVenda = DateTime.UtcNow;
        // Começa como Erro — o service só chama MarcarComoProcessada() depois que a
        // baixa de estoque/status local realmente é aplicada com sucesso (ver
        // MercadoLivreSincronizacaoService.ProcessarNotificacaoWebhookAsync).
        StatusSincronizacao = StatusSincronizacaoVendaMercadoLivre.Erro;
    }

    public void MarcarComoProcessada()
    {
        StatusSincronizacao = StatusSincronizacaoVendaMercadoLivre.Processada;
        ErroSincronizacao = null;
    }

    public void MarcarComoErro(string mensagem)
    {
        StatusSincronizacao = StatusSincronizacaoVendaMercadoLivre.Erro;
        ErroSincronizacao = mensagem;
    }
}
