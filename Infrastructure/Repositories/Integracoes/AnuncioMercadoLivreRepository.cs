using CarStoreManager.Domain.Entities.Integracoes;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class AnuncioMercadoLivreRepository : IAnuncioMercadoLivreRepository
{
    private readonly AppDbContext _context;

    public AnuncioMercadoLivreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AnuncioMercadoLivre?> GetByIdAsync(Guid id)
        => await _context.AnunciosMercadoLivre.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<AnuncioMercadoLivre>> GetAllAsync()
        => await _context.AnunciosMercadoLivre
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();

    public async Task<AnuncioMercadoLivre?> ObterPorEntidadeAsync(string entidadeTipo, Guid entidadeId)
        => await _context.AnunciosMercadoLivre
            .FirstOrDefaultAsync(a => a.EntidadeTipo == entidadeTipo && a.EntidadeId == entidadeId);

    public async Task<AnuncioMercadoLivre?> ObterPorItemIdMLAsync(string itemIdML)
        => await _context.AnunciosMercadoLivre
            .FirstOrDefaultAsync(a => a.ItemIdML == itemIdML);

    public async Task AddAsync(AnuncioMercadoLivre anuncio)
        => await _context.AnunciosMercadoLivre.AddAsync(anuncio);

    public void Update(AnuncioMercadoLivre anuncio)
        => _context.AnunciosMercadoLivre.Update(anuncio);

    public void Remove(AnuncioMercadoLivre anuncio)
        => _context.AnunciosMercadoLivre.Remove(anuncio);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
