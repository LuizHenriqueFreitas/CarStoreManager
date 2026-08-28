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
        => await _context.Clientes
            .Include(c => c.Endereco)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Cliente>> GetAllAsync()
        => await _context.Clientes
            .Include(c => c.Endereco)
            .ToListAsync();

    public async Task<Cliente?> ObterPorCpfAsync(string cpf)
        => await _context.Clientes
            .Include(c => c.Endereco)
            .FirstOrDefaultAsync(c => c.Cpf.Numero == cpf);

    public async Task<bool> CpfExisteAsync(string cpf)
        => await _context.Clientes
            .AnyAsync(c => c.Cpf.Numero == cpf);

    public async Task<List<Cliente>> PesquisarAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return new List<Cliente>();

        var termoLower = termo.Trim().ToLower();
        // CPF é armazenado sem formatação; reduz o termo a dígitos para casar.
        var termoCpf = new string(termo.Where(char.IsDigit).ToArray());

        return await _context.Clientes
            .Include(c => c.Endereco)
            .Where(c => c.Nome.ToLower().Contains(termoLower)
                     || (termoCpf.Length > 0 && c.Cpf.Numero.Contains(termoCpf)))
            .Take(20)
            .ToListAsync();
    }

    public async Task AddAsync(Cliente cliente)
        => await _context.Clientes.AddAsync(cliente);

    public void Update(Cliente cliente)
        => _context.Clientes.Update(cliente);

    // Cliente é dono da FK (EnderecoId) — remover o cliente não apaga o
    // endereço sozinho (cascade só anda de principal pra dependente), por
    // isso o endereço é removido explicitamente junto.
    public void Remove(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        if (cliente.Endereco is not null)
            _context.Enderecos.Remove(cliente.Endereco);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}