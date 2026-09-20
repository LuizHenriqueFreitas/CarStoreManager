using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Enums;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VeiculoVendaRepository : IVeiculoVendaRepository
{
    private readonly AppDbContext _context;

    public VeiculoVendaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<VeiculoVenda?> GetByIdAsync(Guid id)
        => await _context.VeiculosVenda
            .Include(v => v.Fotos)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<VeiculoVenda>> GetAllAsync()
        => await _context.VeiculosVenda
            .Include(v => v.Fotos)
            .ToListAsync();

    public async Task<IEnumerable<VeiculoVenda>> ObterDisponiveisAsync()
        => await _context.VeiculosVenda
            .Include(v => v.Fotos)
            .Where(v => v.Disponibilidade == DisponibilidadeVeiculo.Disponivel)
            .ToListAsync();

    public async Task<IEnumerable<VeiculoVenda>> ObterPorDisponibilidadeAsync(DisponibilidadeVeiculo disponibilidade)
        => await _context.VeiculosVenda
            .Where(v => v.Disponibilidade == disponibilidade)
            .ToListAsync();

    public async Task<VeiculoVenda?> ObterPorPlacaAsync(string placa)
        => await _context.VeiculosVenda
            .FirstOrDefaultAsync(v => v.Placa.Valor == placa);

    public async Task<IEnumerable<VeiculoVenda>> PesquisarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return new List<VeiculoVenda>();

        var termoLower = termo.Trim().ToLower();
        // Placa é armazenada normalizada (maiúscula, sem hífen/espaço).
        var termoPlaca = termo.Replace("-", "").Replace(" ", "").Trim().ToUpper();

        return await _context.VeiculosVenda
            .Include(v => v.Fotos)
            .Where(v => v.Marca.ToLower().Contains(termoLower)
                     || v.Modelo.ToLower().Contains(termoLower)
                     || (termoPlaca.Length > 0 && v.Placa.Valor.Contains(termoPlaca)))
            .Take(20)
            .ToListAsync();
    }

    public async Task AddAsync(VeiculoVenda veiculo)
        => await _context.VeiculosVenda.AddAsync(veiculo);

    public void Update(VeiculoVenda veiculo)
        => _context.VeiculosVenda.Update(veiculo);

    public void Remove(VeiculoVenda veiculo)
        => _context.VeiculosVenda.Remove(veiculo);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}