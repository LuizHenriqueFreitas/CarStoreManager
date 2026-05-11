using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class FornecedorRepository : IFornecedorRepository
{
    private readonly AppDbContext _context;

    public FornecedorRepository(AppDbContext context) => _context = context;

    public async Task<Fornecedor?> GetByIdAsync(Guid id)
        => await _context.Fornecedores.FindAsync(id);

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
        => await _context.Fornecedores.ToListAsync();

    public async Task<Fornecedor?> ObterPorCnpjAsync(string cnpjNumeros)
    {
        // Cnpj é ValueObject (OwnedType) com Numero — comparação por coluna mapeada "Cnpj".
        return await _context.Fornecedores
            .FirstOrDefaultAsync(f => f.Cnpj.Numero == cnpjNumeros);
    }

    public async Task AddAsync(Fornecedor entity) => await _context.Fornecedores.AddAsync(entity);
    public void Update(Fornecedor entity) => _context.Fornecedores.Update(entity);
    public void Remove(Fornecedor entity) => _context.Fornecedores.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
