using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class PropostaVendaRepository : IPropostaVendaRepository
{
    private readonly AppDbContext _context;

    public PropostaVendaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropostaVenda?> GetByIdAsync(Guid id)
        => await _context.PropostasVenda
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<PropostaVenda>> GetAllAsync()
        => await _context.PropostasVenda
            .ToListAsync();

    public async Task<IEnumerable<PropostaVenda>> ObterPorVendedorAsync(Guid vendedorId)
        => await _context.PropostasVenda
            .Where(p => p.VendedorId == vendedorId)
            .ToListAsync();

    public async Task<IEnumerable<PropostaVenda>> ObterPorClienteAsync(Guid clienteId)
        => await _context.PropostasVenda
            .Where(p => p.ClienteId == clienteId)
            .ToListAsync();

    public async Task<IEnumerable<PropostaVenda>> ObterPorStatusAsync(StatusPropostaVenda status)
        => await _context.PropostasVenda
            .Where(p => p.Status == status)
            .ToListAsync();

    public async Task AddAsync(PropostaVenda proposta)
        => await _context.PropostasVenda.AddAsync(proposta);

    public void Update(PropostaVenda proposta)
        => _context.PropostasVenda.Update(proposta);

    public void Remove(PropostaVenda proposta)
        => _context.PropostasVenda.Remove(proposta);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}