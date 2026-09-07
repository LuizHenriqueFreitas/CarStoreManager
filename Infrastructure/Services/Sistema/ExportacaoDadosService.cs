using System.Linq;
using System.Reflection;
using System.Text.Json;
using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarStoreManager.Infrastructure.Services.Sistema;

/// <summary>
/// Varre todas as entidades mapeadas no <see cref="AppDbContext"/> (menos as
/// owned, que já vão aninhadas no dono) e serializa cada tabela como um array
/// JSON, tudo dentro de um único objeto { "Clientes": [...], "OrdensServico":
/// [...] }. Leitura sem tracking; ciclos de navegação são ignorados.
/// </summary>
public sealed class ExportacaoDadosService : IExportacaoDadosService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ExportacaoDadosService> _logger;

    private static readonly JsonSerializerOptions Opcoes = new()
    {
        WriteIndented = true,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
    };

    private static readonly MethodInfo SetGenerico = typeof(DbContext).GetMethods()
        .First(m => m.Name == nameof(DbContext.Set) && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

    public ExportacaoDadosService(AppDbContext db, ILogger<ExportacaoDadosService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<ExportacaoDadosArquivo>> ExportarJsonAsync(CancellationToken ct = default)
    {
        try
        {
            _db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var raiz = new Dictionary<string, object>
            {
                ["_meta"] = new
                {
                    aplicacao = "DELORE",
                    geradoEm = DateTime.UtcNow,
                    versaoFormato = 1,
                },
            };

            var tipos = _db.Model.GetEntityTypes()
                .Where(t => !t.IsOwned())
                .Select(t => t.ClrType)
                .Distinct()
                .OrderBy(t => t.Name);

            foreach (var clr in tipos)
            {
                ct.ThrowIfCancellationRequested();

                var query = (IQueryable)SetGenerico.MakeGenericMethod(clr).Invoke(_db, null)!;
                var registros = new List<object>();
                await foreach (var item in query.Cast<object>().AsAsyncEnumerable().WithCancellation(ct))
                    registros.Add(item);

                raiz[clr.Name] = registros;
            }

            var json = JsonSerializer.SerializeToUtf8Bytes(raiz, Opcoes);
            var nome = $"delore-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.json";
            return Result<ExportacaoDadosArquivo>.Ok(new ExportacaoDadosArquivo(nome, json));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao exportar o banco de dados.");
            return Result<ExportacaoDadosArquivo>.Fail("Não foi possível gerar o arquivo de exportação.");
        }
    }
}
