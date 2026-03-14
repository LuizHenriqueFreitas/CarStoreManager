using CarStoreManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SistemaEmpresa.Domain.Entities;
using SistemaEmpresa.Domain.Entities.Concessionaria;
using SistemaEmpresa.Domain.Entities.Oficina;

namespace CarStoreManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }

    public DbSet<Veiculo> Veiculos { get; set; }

    public DbSet<Peca> Pecas { get; set; }

    public DbSet<OrdemServico> OrdensServico { get; set; }

    public DbSet<PropostaVenda> PropostasVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>().HasKey(c => c.Id);

        modelBuilder.Entity<Veiculo>().HasKey(v => v.Id);

        modelBuilder.Entity<Peca>().HasKey(p => p.Id);

        modelBuilder.Entity<OrdemServico>().HasKey(o => o.Id);

        modelBuilder.Entity<PropostaVenda>().HasKey(p => p.Id);
    }
}