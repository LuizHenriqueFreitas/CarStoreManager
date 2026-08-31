using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Sobrescreve campos de data (DataCriacao, PrazoEstimado, etc.) DEPOIS que a
/// entidade já foi criada por um Application Service normal — necessário
/// porque essas propriedades são <c>protected set</c>, atribuídas automaticamente
/// como DateTime.UtcNow no construtor, sem parâmetro para vir de fora.
///
/// Usa DbContext.Entry(...).Property(...).CurrentValue, que escreve no valor
/// via metadados do EF Core (contorna o setter protegido do C#, sem refletir
/// diretamente) — é o único jeito de "datar no passado" um registro sem abrir
/// mão de criar tudo pelas regras de negócio reais do service.
/// </summary>
public static class BackdateHelper
{
    public static async Task AplicarAsync<TEntity>(
        IServiceProvider provider, Guid id, params (string Propriedade, object? Valor)[] valores)
        where TEntity : class
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var entidade = await context.Set<TEntity>().FindAsync(id);
        if (entidade is null) return;

        var entry = context.Entry(entidade);
        foreach (var (propriedade, valor) in valores)
            entry.Property(propriedade).CurrentValue = valor;

        await context.SaveChangesAsync();
    }

    /// <summary>Data aleatória dentro de [inicio, fim], inclusive.</summary>
    public static DateTime DataAleatoriaNoPeriodo(Random rng, DateTime inicio, DateTime fim)
    {
        var dias = (fim.Date - inicio.Date).Days;
        return inicio.Date.AddDays(rng.Next(0, Math.Max(1, dias + 1))).AddHours(rng.Next(8, 19));
    }
}
