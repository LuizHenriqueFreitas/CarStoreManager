using CarStoreManager.Application.Common;

namespace CarStoreManager.Application.Interfaces.Sistema;

/// <summary>
/// Abstração de envio de e-mail. A implementação (SMTP real) lê as credenciais
/// dinamicamente das ConfiguracoesSistema do BD — não depende de appsettings.
/// </summary>
public interface IEmailService
{
    Task<Result> EnviarAsync(string destinatario, string assunto, string corpo, bool isHtml = true);
}
