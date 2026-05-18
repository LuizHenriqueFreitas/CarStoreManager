using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class ConfiguracaoSistemaService : IConfiguracaoSistemaService
{
    private readonly IConfiguracaoSistemaRepository _repo;
    private readonly IEmailService _emailService;

    public ConfiguracaoSistemaService(
        IConfiguracaoSistemaRepository repo,
        IEmailService emailService)
    {
        _repo = repo;
        _emailService = emailService;
    }

    public async Task<Result<ConfiguracaoSistemaDTO>> ObterAsync()
    {
        var cfg = await _repo.ObterAsync();
        return Result<ConfiguracaoSistemaDTO>.Ok(MapToDto(cfg));
    }

    public async Task<Result> AtualizarAsync(ConfiguracaoSistemaDTO dto)
    {
        var cfg = await _repo.ObterAsync();
        try
        {
            cfg.AtualizarFinanciadora(dto.NomeFinanciadora, dto.EmailFinanciadora);

            // Senha vazia no DTO = manter a senha atual (não sobrescrever).
            var senha = string.IsNullOrEmpty(dto.SmtpSenha) ? cfg.SmtpSenha : dto.SmtpSenha;

            cfg.AtualizarSmtp(
                host: dto.SmtpHost,
                porta: dto.SmtpPort,
                usuario: dto.SmtpUsuario,
                senha: senha,
                usarSsl: dto.SmtpUsarSsl,
                emailRemetente: dto.EmailRemetente,
                nomeRemetente: dto.NomeRemetente);

            cfg.ConfigurarEntradaMinima(dto.ExigirEntradaMinima, dto.PercentualEntradaMinima);

            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result> TestarEnvioAsync(string emailDestino)
    {
        var assunto = "[CarStore] Teste de configuração SMTP";
        var corpo = """
            <h2>Teste de envio</h2>
            <p>Este e-mail foi disparado pelo botão "Testar envio" da tela de configurações
            do CarStore Manager.</p>
            <p>Se você recebeu, suas credenciais SMTP estão funcionando corretamente.</p>
            """;
        return await _emailService.EnviarAsync(emailDestino, assunto, corpo, isHtml: true);
    }

    public async Task<Result<MargensDTO>> ObterMargensAsync()
    {
        var cfg = await _repo.ObterAsync();
        Dictionary<string, decimal> margens;
        try
        {
            margens = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, decimal>>(cfg.MargensPorSistemaJson)
                ?? new();
        }
        catch { margens = new(); }

        return Result<MargensDTO>.Ok(new MargensDTO
        {
            MargensPorSistema = margens,
            MargemPadraoGlobalPct = cfg.MargemPadraoGlobalPct
        });
    }

    public async Task<Result> AtualizarMargensAsync(MargensDTO dto)
    {
        var cfg = await _repo.ObterAsync();
        try
        {
            cfg.AtualizarMargens(dto.MargensPorSistema, dto.MargemPadraoGlobalPct);
            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex) { return Result.Fail(ex.Message); }
    }

    private static ConfiguracaoSistemaDTO MapToDto(ConfiguracaoSistema cfg) => new()
    {
        NomeFinanciadora = cfg.NomeFinanciadora,
        EmailFinanciadora = cfg.EmailFinanciadora,
        SmtpHost = cfg.SmtpHost,
        SmtpPort = cfg.SmtpPort,
        SmtpUsuario = cfg.SmtpUsuario,
        SmtpSenha = "", // nunca devolvemos a senha — campo só pra POST
        SmtpUsarSsl = cfg.SmtpUsarSsl,
        EmailRemetente = cfg.EmailRemetente,
        NomeRemetente = cfg.NomeRemetente,
        DataUltimaAtualizacao = cfg.DataUltimaAtualizacao,
        SmtpConfigurado = cfg.SmtpConfigurado(),
        FinanciadoraConfigurada = cfg.FinanciadoraConfigurada(),
        ExigirEntradaMinima = cfg.ExigirEntradaMinima,
        PercentualEntradaMinima = cfg.PercentualEntradaMinima
    };
}
