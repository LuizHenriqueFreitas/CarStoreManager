using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(Guid id)
        => await _context.Clientes.FindAsync(id);

    public async Task<IEnumerable<Cliente>> GetAllAsync()
        => await _context.Clientes.ToListAsync();

    public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        => await _context.Clientes
            .FirstOrDefaultAsync(c => c.CPF.Numero == cpf);

    public async Task<bool> CpfExisteAsync(string cpf)
        => await _context.Clientes
            .AnyAsync(c => c.CPF.Numero == cpf);

    public async Task AddAsync(Cliente cliente)
        => await _context.Clientes.AddAsync(cliente);

    public void Update(Cliente cliente)
        => _context.Clientes.Update(cliente);

    public void Remove(Cliente cliente)
        => _context.Clientes.Remove(cliente);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}