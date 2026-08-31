using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Infrastructure.Data;

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
            entry.Property(propriedade).CurrentValue = valor;

        await _context.SaveChangesAsync();
    }
}
