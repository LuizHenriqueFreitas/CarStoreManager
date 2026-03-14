using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SistemaEmpresa.Domain.Entities.Concessionaria;

namespace CarStoreManager.Infrastructure.Repositories;

public class PropostaVendaRepository : IPropostaVendaRepository
{
    private readonly AppDbContext _context;

    public PropostaVendaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PropostaVenda>> ObterTodasAsync()
    {
        return await _context.PropostasVenda
            .Include(p => p.ClienteId)
            .Include(p => p.VeiculoId)
            .ToListAsync();
    }

    public async Task<PropostaVenda?> ObterPorIdAsync(Guid id)
    {
        return await _context.PropostasVenda
            .Include(p => p.ClienteId)
            .Include(p => p.VeiculoId)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AdicionarAsync(PropostaVenda proposta)
    {
        await _context.PropostasVenda.AddAsync(proposta);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(PropostaVenda proposta)
    {
        _context.PropostasVenda.Update(proposta);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var proposta = await _context.PropostasVenda.FindAsync(id);

        if (proposta != null)
        {
            _context.PropostasVenda.Remove(proposta);
            await _context.SaveChangesAsync();
        }
    }
}