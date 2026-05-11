using System.Net;
using System.Net.Mail;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Infrastructure.Services;

/// <summary>
/// Envia e-mail via SMTP usando as credenciais salvas em ConfiguracaoSistema.
/// Se nada estiver configurado, falha cedo com mensagem clara — não silencia.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguracaoSistemaRepository _configRepo;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IConfiguracaoSistemaRepository configRepo,
        ILogger<SmtpEmailService> logger)
    {
        _configRepo = configRepo;
        _logger = logger;
    }

    public async Task<Result> EnviarAsync(string destinatario, string assunto, string corpo, bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(destinatario) || !destinatario.Contains('@'))
            return Result.Fail("Destinatário inválido.");

        var cfg = await _configRepo.ObterAsync();
        if (!cfg.SmtpConfigurado())
            return Result.Fail("SMTP não configurado. Acesse Configurações do sistema antes de enviar e-mails.");

        try
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(cfg.EmailRemetente,
                    string.IsNullOrWhiteSpace(cfg.NomeRemetente) ? cfg.EmailRemetente : cfg.NomeRemetente),
                Subject = assunto,
                Body = corpo,
                IsBodyHtml = isHtml
            };
            msg.To.Add(new MailAddress(destinatario));

            using var client = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
            {
                EnableSsl = cfg.SmtpUsarSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            if (!string.IsNullOrWhiteSpace(cfg.SmtpUsuario))
                client.Credentials = new NetworkCredential(cfg.SmtpUsuario, cfg.SmtpSenha);

            await client.SendMailAsync(msg);
            _logger.LogInformation("E-mail enviado para {Destinatario}: {Assunto}", destinatario, assunto);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {Destinatario}", destinatario);
            return Result.Fail($"Falha SMTP: {ex.Message}");
        }
    }
}
