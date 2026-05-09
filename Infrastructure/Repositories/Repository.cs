using Microsoft.EntityFrameworkCore;
using CarStoreManager.Domain.Base;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;

namespace CarStoreManager.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}