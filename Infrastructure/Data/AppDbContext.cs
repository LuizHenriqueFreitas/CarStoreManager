using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CarStoreManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }

    public DbSet<Veiculo> Veiculos { get; set; }

    public DbSet<Componente> Componentes { get; set; }

    public DbSet<OrdemServico> OrdensServico { get; set; }

    public DbSet<PropostaVenda> PropostasVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>().HasKey(c => c.Id);

        modelBuilder.Entity<Veiculo>().HasKey(v => v.Id);
        
        modelBuilder.Entity<Veiculo>().OwnsOne(v => v.Placa);

        modelBuilder.Entity<Componente>().HasKey(p => p.Id);

        modelBuilder.Entity<OrdemServico>().HasKey(o => o.Id);

        modelBuilder.Entity<PropostaVenda>().HasKey(p => p.Id);

        modelBuilder.Owned<PlacaVeiculo>();
    }
}