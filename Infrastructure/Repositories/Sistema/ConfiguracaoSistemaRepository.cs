using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Sistema;

public class ConfiguracaoSistemaRepository : IConfiguracaoSistemaRepository
{
    private readonly AppDbContext _context;

    public ConfiguracaoSistemaRepository(AppDbContext context) => _context = context;

    public async Task<ConfiguracaoSistema> ObterAsync()
    {
        var existente = await _context.ConfiguracoesSistema.FirstOrDefaultAsync();
        if (existente is not null) return existente;

        // Auto-cria registro vazio na primeira chamada — admin preenche via UI.
        var nova = new ConfiguracaoSistema(true);
        await _context.ConfiguracoesSistema.AddAsync(nova);
        await _context.SaveChangesAsync();
        return nova;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
