using CarStoreManager.Application.Interfaces.Sistema;
using System.Linq.Expressions;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Services;

public class BackdateService : IBackdateService
{
    private readonly AppDbContext _context;

    public BackdateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AplicarAsync<TEntity>(Guid id, params (string Propriedade, object? Valor)[] valores)
        where TEntity : class
    {
        var entidade = await _context.Set<TEntity>().FindAsync(id);
        if (entidade is null) return;

        var entry = _context.Entry(entidade);
        foreach (var (propriedade, valor) in valores)
            Definir(entry, propriedade, valor);

        await _context.SaveChangesAsync();
    }

    public async Task<int> AplicarOndeAsync<TEntity>(Expression<Func<TEntity, bool>> filtro, params (string Propriedade, object? Valor)[] valores)
        where TEntity : class
    {
        var entidades = await _context.Set<TEntity>().Where(filtro).ToListAsync();
        if (entidades.Count == 0) return 0;

        foreach (var entidade in entidades)
        {
            var entry = _context.Entry(entidade);
            foreach (var (propriedade, valor) in valores)
                Definir(entry, propriedade, valor);
        }

        await _context.SaveChangesAsync();
        return entidades.Count;
    }

    public async Task<List<TResultado>> ConsultarAsync<TEntity, TResultado>(
        Expression<Func<TEntity, bool>> filtro, Expression<Func<TEntity, TResultado>> projecao)
        where TEntity : class
        => await _context.Set<TEntity>().AsNoTracking().Where(filtro).Select(projecao).ToListAsync();

    /// <summary>
    /// Aceita caminho com ponto pra tipo owned (ex.:
    /// "DadosFuncionario.DataContratacao" num Usuario).
    /// </summary>
    private static void Definir(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propriedade, object? valor)
    {
        var partes = propriedade.Split('.');
        for (var i = 0; i < partes.Length - 1; i++)
        {
            var alvo = entry.Reference(partes[i]).TargetEntry;
            if (alvo is null) return;
            entry = alvo;
        }
        entry.Property(partes[^1]).CurrentValue = valor;
    }

    public void LimparRastreamento() => _context.ChangeTracker.Clear();
}
