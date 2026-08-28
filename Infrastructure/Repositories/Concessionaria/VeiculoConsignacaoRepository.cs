using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VeiculoConsignacaoRepository : IVeiculoConsignacaoRepository
{
    private readonly AppDbContext _context;

    public VeiculoConsignacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VeiculoConsignacao?> GetByIdAsync(Guid id)
        => await _context.VeiculosConsignacao
            .Include(v => v.Historico)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<VeiculoConsignacao>> GetAllAsync()
        => await _context.VeiculosConsignacao
            .Include(v => v.Historico)
            .ToListAsync();

    public async Task<IEnumerable<VeiculoConsignacao>> ObterPorStatusAsync(StatusConsignacao status)
        => await _context.VeiculosConsignacao
            .Where(v => v.Status == status)
            .ToListAsync();

    public async Task<IEnumerable<VeiculoConsignacao>> ObterPorClienteAsync(Guid clienteProprietarioId)
        => await _context.VeiculosConsignacao
            .Where(v => v.ClienteProprietarioId == clienteProprietarioId)
            .ToListAsync();

    public async Task<IEnumerable<VeiculoConsignacao>> ObterPorVendedorAsync(Guid vendedorResponsavelId)
        => await _context.VeiculosConsignacao
            .Where(v => v.VendedorResponsavelId == vendedorResponsavelId)
            .ToListAsync();

    public async Task AddAsync(VeiculoConsignacao veiculo)
        => await _context.VeiculosConsignacao.AddAsync(veiculo);

    public void Update(VeiculoConsignacao veiculo)
        => _context.VeiculosConsignacao.Update(veiculo);

    public void Remove(VeiculoConsignacao veiculo)
        => _context.VeiculosConsignacao.Remove(veiculo);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
