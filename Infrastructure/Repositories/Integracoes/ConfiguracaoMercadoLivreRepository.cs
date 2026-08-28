using CarStoreManager.Domain.Entities.Integracoes;
using CarStoreManager.Domain.Interfaces.Repositories.Integracoes;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories.Integracoes;

public class ConfiguracaoMercadoLivreRepository : IConfiguracaoMercadoLivreRepository
{
    private readonly AppDbContext _context;

    public ConfiguracaoMercadoLivreRepository(AppDbContext context) => _context = context;

    public async Task<ConfiguracaoMercadoLivre> ObterAsync()
    {
        var existente = await _context.ConfiguracoesMercadoLivre.FirstOrDefaultAsync();
        if (existente is not null) return existente;

        // Auto-cria registro vazio na primeira chamada — admin conecta via OAuth depois.
        var nova = new ConfiguracaoMercadoLivre(true);
        await _context.ConfiguracoesMercadoLivre.AddAsync(nova);
        await _context.SaveChangesAsync();
        return nova;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
