using CarStoreManager.Application.DTOs.Integracoes.MercadoLivre;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Domain.Interfaces.Repositories.Integracoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Web.Controllers;

/// <summary>
/// Recebe notificações de venda do Mercado Livre. Mesmo padrão de segurança já
/// usado por PropostaVendaController para o link público do termo de entrega:
/// [AllowAnonymous] + rota absoluta com um segredo embutido, IP registrado.
/// Sempre responde 200 quando a notificação foi consultada com sucesso — mesmo
/// que a aplicação local (baixa de estoque, etc) tenha falhado — porque nesse
/// caso reenviar o webhook não resolveria nada. Só sinaliza falha quando nem a
/// consulta do pedido no ML deu certo (aí vale a pena o ML tentar de novo).
/// </summary>
[ApiController]
[Route("api/mercadolivre")]
[Authorize(Roles = "Admin")]
public class MercadoLivreWebhookController : ControllerBase
{
    private readonly IMercadoLivreSincronizacaoService _service;
    private readonly IConfiguracaoMercadoLivreRepository _configRepo;
    private readonly ILogger<MercadoLivreWebhookController> _logger;

    public MercadoLivreWebhookController(
        IMercadoLivreSincronizacaoService service,
        IConfiguracaoMercadoLivreRepository configRepo,
        ILogger<MercadoLivreWebhookController> logger)
    {
        _service = service;
        _configRepo = configRepo;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("/api/mercadolivre/webhook/{secret}")]
    public async Task<IActionResult> Receber(string secret, [FromBody] MercadoLivreWebhookNotificacaoDTO notificacao)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
        var cfg = await _configRepo.ObterAsync();

        if (string.IsNullOrEmpty(cfg.SegredoWebhook) || cfg.SegredoWebhook != secret)
        {
            // Não revela se o segredo é inválido via status code diferente — apenas
            // não processa e loga o IP para investigação manual.
            _logger.LogWarning("Webhook Mercado Livre recebido com segredo inválido. IP: {Ip}", ip);
            return Ok();
        }

        _logger.LogInformation("Webhook Mercado Livre recebido. IP: {Ip}, Topic: {Topic}, Resource: {Resource}", ip, notificacao.Topic, notificacao.Resource);

        var r = await _service.ProcessarNotificacaoWebhookAsync(notificacao);
        return r.IsSuccess ? Ok() : StatusCode(503, r.Error);
    }
}
