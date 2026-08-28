using CarStoreManager.Domain.Entities.Integracoes;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VendaMercadoLivreRepository : IVendaMercadoLivreRepository
{
    private readonly AppDbContext _context;

    public VendaMercadoLivreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VendaMercadoLivre?> GetByIdAsync(Guid id)
        => await _context.VendasMercadoLivre.FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<VendaMercadoLivre>> GetAllAsync()
        => await _context.VendasMercadoLivre
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

    public async Task<VendaMercadoLivre?> ObterPorPedidoEItemAsync(string idPedidoPlataforma, string idItemPlataforma)
        => await _context.VendasMercadoLivre
            .FirstOrDefaultAsync(v => v.IdPedidoPlataforma == idPedidoPlataforma && v.IdItemPlataforma == idItemPlataforma);

    public async Task<IEnumerable<VendaMercadoLivre>> ObterComErroAsync()
        => await _context.VendasMercadoLivre
            .Where(v => v.StatusSincronizacao == StatusSincronizacaoVendaMercadoLivre.Erro)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();

    public async Task AddAsync(VendaMercadoLivre venda)
        => await _context.VendasMercadoLivre.AddAsync(venda);

    public void Update(VendaMercadoLivre venda)
        => _context.VendasMercadoLivre.Update(venda);

    public void Remove(VendaMercadoLivre venda)
        => _context.VendasMercadoLivre.Remove(venda);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
