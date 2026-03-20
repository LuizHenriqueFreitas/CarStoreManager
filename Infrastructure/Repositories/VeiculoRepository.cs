using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly AppDbContext _context;

    public VeiculoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Veiculo>> ObterTodosAsync()
    {
        return await _context.Veiculos.ToListAsync();
    }

    public async Task<Veiculo?> ObterPorIdAsync(Guid id)
    {
        return await _context.Veiculos.FindAsync(id);
    }

    public async Task<IEnumerable<Veiculo>> ObterPorClienteAsync(Guid clienteId)
    {
        return await _context.Veiculos.ToListAsync();
    }

    public async Task AdicionarAsync(Veiculo v)
    {
        await _context.Veiculos.AddAsync(v);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Veiculo v)
    {
        _context.Veiculos.Update(v);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);

        if (veiculo != null)
        {
            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
        }
    }
}