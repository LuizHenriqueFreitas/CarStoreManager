using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VeiculoClienteRepository : IVeiculoClienteRepository
{
    private readonly AppDbContext _context;

    public VeiculoClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VeiculoCliente?> GetByIdAsync(Guid id)
        => await _context.VeiculosCliente
            .Include(v => v.HistoricoServicos)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<VeiculoCliente>> GetAllAsync()
        => await _context.VeiculosCliente.ToListAsync();

    public async Task<IEnumerable<VeiculoCliente>> ObterPorClienteAsync(Guid clienteId)
        => await _context.VeiculosCliente
            .Where(v => v.ClienteId == clienteId)
            .ToListAsync();

    public async Task<List<VeiculoCliente>> PesquisarAsync(string termo, Guid? clienteId = null)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return new List<VeiculoCliente>();

        var t = termo.Trim().ToLower();
        var q = _context.VeiculosCliente.AsQueryable();
        if (clienteId.HasValue && clienteId.Value != Guid.Empty)
            q = q.Where(v => v.ClienteId == clienteId.Value);

        return await q
            .Where(v => v.Marca.ToLower().Contains(t)
                     || v.Modelo.ToLower().Contains(t)
                     || v.Cor.ToLower().Contains(t)
                     || v.Placa.Valor.ToLower().Contains(t))
            .Take(20)
            .ToListAsync();
    }

    public async Task AddAsync(VeiculoCliente veiculo)
        => await _context.VeiculosCliente.AddAsync(veiculo);

    public void Update(VeiculoCliente veiculo)
        => _context.VeiculosCliente.Update(veiculo);

    public void Remove(VeiculoCliente veiculo)
        => _context.VeiculosCliente.Remove(veiculo);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}