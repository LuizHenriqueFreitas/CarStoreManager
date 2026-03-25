using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id)
        => await _context.Usuarios.FindAsync(id);

    public async Task<IEnumerable<Usuario>> GetAllAsync()
        => await _context.Usuarios.ToListAsync();

    public async Task<Usuario?> ObterPorEmailAsync(string email)
        => await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.Endereco == email.ToLower());

    public async Task<bool> EmailExisteAsync(string email)
        => await _context.Usuarios
            .AnyAsync(u => u.Email.Endereco == email.ToLower());

    public async Task AddAsync(Usuario usuario)
        => await _context.Usuarios.AddAsync(usuario);

    public void Update(Usuario usuario)
        => _context.Usuarios.Update(usuario);

    public void Remove(Usuario usuario)
        => _context.Usuarios.Remove(usuario);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}