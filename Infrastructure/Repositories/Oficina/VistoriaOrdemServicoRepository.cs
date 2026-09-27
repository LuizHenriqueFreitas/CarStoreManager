using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class VistoriaOrdemServicoRepository : IVistoriaOrdemServicoRepository
{
    private readonly AppDbContext _context;

    public VistoriaOrdemServicoRepository(AppDbContext context) => _context = context;

    public async Task<VistoriaOrdemServico?> GetByIdAsync(Guid id) => await _context.VistoriasOrdemServico.FindAsync(id);
    public async Task<IEnumerable<VistoriaOrdemServico>> GetAllAsync() => await _context.VistoriasOrdemServico.ToListAsync();

    public async Task<VistoriaOrdemServico?> ObterPorOrdemServicoAsync(Guid ordemServicoId)
        => await _context.VistoriasOrdemServico.FirstOrDefaultAsync(v => v.OrdemServicoId == ordemServicoId);

    public async Task AddAsync(VistoriaOrdemServico entity) => await _context.VistoriasOrdemServico.AddAsync(entity);
    public void Update(VistoriaOrdemServico entity) => _context.VistoriasOrdemServico.Update(entity);
    public void Remove(VistoriaOrdemServico entity) => _context.VistoriasOrdemServico.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
