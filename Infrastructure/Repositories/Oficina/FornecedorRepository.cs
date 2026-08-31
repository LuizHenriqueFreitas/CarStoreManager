using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class FornecedorRepository : IFornecedorRepository
{
    private readonly AppDbContext _context;

    public FornecedorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Fornecedor?> GetByIdAsync(Guid id)
        => await _context.Fornecedores
            .Include(f => f.Endereco)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
        => await _context.Fornecedores
            .Include(f => f.Endereco)
            .OrderBy(f => f.Nome)
            .ToListAsync();

    public async Task<bool> CnpjExisteAsync(string cnpj)
    {
        var digitos = new string((cnpj ?? "").Where(char.IsDigit).ToArray());
        return await _context.Fornecedores
            .AnyAsync(f => f.Cnpj.Numero == digitos);
    }

    public async Task<IEnumerable<Fornecedor>> BuscarAsync(string termo, int limite = 20)
    {
        if (limite <= 0) limite = 20;
        if (limite > 100) limite = 100;

        var query = _context.Fornecedores.Where(f => f.Ativo);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim().ToLower();
            var tDigitos = new string(termo.Where(char.IsDigit).ToArray());
            query = query.Where(f =>
                f.Nome.ToLower().Contains(t) ||
                (tDigitos.Length > 0 && f.Cnpj.Numero.Contains(tDigitos)));
        }

        return await query
            .OrderBy(f => f.Nome)
            .Take(limite)
            .ToListAsync();
    }

    public async Task AddAsync(Fornecedor fornecedor)
        => await _context.Fornecedores.AddAsync(fornecedor);

    public void Update(Fornecedor fornecedor)
        => _context.Fornecedores.Update(fornecedor);

    // Fornecedor é dono da FK (EnderecoId) — remover o fornecedor não apaga
    // o endereço sozinho, por isso é removido explicitamente junto (mesmo
    // raciocínio do ClienteRepository.Remove).
    public void Remove(Fornecedor fornecedor)
    {
        _context.Fornecedores.Remove(fornecedor);
        if (fornecedor.Endereco is not null)
            _context.Enderecos.Remove(fornecedor.Endereco);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
