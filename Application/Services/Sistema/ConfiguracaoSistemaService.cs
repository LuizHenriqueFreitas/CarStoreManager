using CarStoreManager.Application.Common;
using CarStoreManager.Application.DTOs.Sistema;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;

namespace CarStoreManager.Application.Services.Sistema;

public class ConfiguracaoSistemaService : IConfiguracaoSistemaService
{
    private readonly IConfiguracaoSistemaRepository _repo;

    public ConfiguracaoSistemaService(IConfiguracaoSistemaRepository repo)
    {
        _repo = repo;
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
            cfg.ConfigurarEntradaMinima(dto.ExigirEntradaMinima, dto.PercentualEntradaMinima);

            await _repo.SaveChangesAsync();
            return Result.Ok();
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception)
        {
            return Result.Fail("Não foi possível salvar a configuração. Tente novamente em instantes.");
        }
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
        catch (Exception) { return Result.Fail("Não foi possível salvar as margens. Tente novamente em instantes."); }
    }

    private static ConfiguracaoSistemaDTO MapToDto(ConfiguracaoSistema cfg) => new()
    {
        DataUltimaAtualizacao = cfg.DataUltimaAtualizacao,
        ExigirEntradaMinima = cfg.ExigirEntradaMinima,
        PercentualEntradaMinima = cfg.PercentualEntradaMinima
    };
}
