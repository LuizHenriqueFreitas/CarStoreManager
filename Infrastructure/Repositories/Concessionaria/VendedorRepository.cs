using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VendedorRepository : IVendedorRepository
{
    private readonly AppDbContext _context;

    public VendedorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vendedor?> GetByIdAsync(Guid id)
        => await _context.Usuarios
            .OfType<Vendedor>()
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<Vendedor>> GetAllAsync()
        => await _context.Usuarios
            .OfType<Vendedor>()
            .ToListAsync();

    public async Task<IEnumerable<Vendedor>> ObterPorNivelAsync(NivelFuncionario nivel)
        => await _context.Usuarios
            .OfType<Vendedor>()
            .Where(v => v.DadosFuncionario.Nivel == nivel)
            .ToListAsync();

    public async Task AddAsync(Vendedor vendedor)
        => await _context.Usuarios.AddAsync(vendedor);

    public void Update(Vendedor vendedor)
        => _context.Usuarios.Update(vendedor);

    public void Remove(Vendedor vendedor)
        => _context.Usuarios.Remove(vendedor);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}