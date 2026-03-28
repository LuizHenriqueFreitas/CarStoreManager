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

    // =========================
    // USUÁRIOS
    // =========================
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<Mecanico> Mecanicos { get; set; }

    // =========================
    // CLIENTES
    // =========================
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<VeiculoCliente> VeiculosCliente { get; set; }

    // =========================
    // OFICINA
    // =========================
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<ItemOrdemServico> ItensOrdemServico { get; set; }
    public DbSet<ChecklistItem> ChecklistItens { get; set; }
    public DbSet<Componente> Componentes { get; set; }

    // =========================
    // CONCESSIONÁRIA
    // =========================
    public DbSet<VeiculoVenda> VeiculosVenda { get; set; }
    public DbSet<FotoVeiculo> FotosVeiculo { get; set; }
    public DbSet<PropostaVenda> PropostasVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // HERANÇA (TPH - Table per Hierarchy)
        // =========================
        modelBuilder.Entity<Usuario>()
            .HasDiscriminator<string>("TipoUsuario")
            .HasValue<Usuario>("Usuario")
            .HasValue<Admin>("Admin")
            .HasValue<Vendedor>("Vendedor")
            .HasValue<Mecanico>("Mecanico");

        // =========================
        // CLIENTE
        // =========================
        modelBuilder.Entity<Cliente>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Cliente>()
            .OwnsOne(c => c.Email);

        modelBuilder.Entity<Cliente>()
            .OwnsOne(c => c.Telefone);

        modelBuilder.Entity<Cliente>()
            .OwnsOne(c => c.CPF);

        // =========================
        // VEICULO CLIENTE
        // =========================
        modelBuilder.Entity<VeiculoCliente>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<VeiculoCliente>()
            .OwnsOne(v => v.Ano);

        modelBuilder.Entity<VeiculoCliente>()
            .HasMany(v => v.HistoricoServicos)
            .WithOne()
            .HasForeignKey("VeiculoId");

        // =========================
        // MECÂNICO
        // =========================
        modelBuilder.Entity<Mecanico>()
            .OwnsOne(m => m.Email);

        modelBuilder.Entity<Mecanico>()
            .OwnsOne(m => m.Telefone);

        modelBuilder.Entity<Mecanico>()
            .OwnsOne(m => m.DadosFuncionario);

        // =========================
        // VENDEDOR
        // =========================
        modelBuilder.Entity<Vendedor>()
            .OwnsOne(v => v.Email);

        modelBuilder.Entity<Vendedor>()
            .OwnsOne(v => v.Telefone);

        modelBuilder.Entity<Vendedor>()
            .OwnsOne(v => v.DadosFuncionario);

        // =========================
        // ORDEM SERVIÇO
        // =========================
        modelBuilder.Entity<OrdemServico>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<OrdemServico>()
            .OwnsOne(o => o.CustoServico);

        modelBuilder.Entity<OrdemServico>()
            .OwnsOne(o => o.ValorTotal);

        modelBuilder.Entity<OrdemServico>()
            .HasMany(o => o.Itens)
            .WithOne()
            .HasForeignKey(i => i.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrdemServico>()
            .HasMany(o => o.Checklist)
            .WithOne()
            .HasForeignKey(c => c.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================
        // ITEM ORDEM SERVIÇO
        // =========================
        modelBuilder.Entity<ItemOrdemServico>()
            .OwnsOne(i => i.ValorUnitario);

        modelBuilder.Entity<ItemOrdemServico>()
            .OwnsOne(i => i.ValorTotal);

        // =========================
        // COMPONENTE
        // =========================
        modelBuilder.Entity<Componente>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Componente>()
            .OwnsOne(c => c.Valor);

        // =========================
        // VEICULO VENDA
        // =========================
        modelBuilder.Entity<VeiculoVenda>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Ano);

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Quilometragem);

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Placa);

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Valor);

        modelBuilder.Entity<VeiculoVenda>()
            .HasMany(v => v.Fotos)
            .WithOne()
            .HasForeignKey(f => f.VeiculoVendaId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================
        // PROPOSTA VENDA
        // =========================
        modelBuilder.Entity<PropostaVenda>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<PropostaVenda>()
            .OwnsOne(p => p.ValorBase);

        modelBuilder.Entity<PropostaVenda>()
            .OwnsOne(p => p.ValorFinal);

        modelBuilder.Entity<PropostaVenda>()
            .OwnsOne(p => p.Entrada);

        modelBuilder.Entity<PropostaVenda>()
            .OwnsOne(p => p.Desconto);

        modelBuilder.Entity<PropostaVenda>()
            .OwnsOne(p => p.Financiamento);

        // =========================
        // VALUE OBJECTS GLOBAIS
        // =========================
        modelBuilder.Owned<Email>();
        modelBuilder.Owned<Telefone>();
        modelBuilder.Owned<Dinheiro>();
        modelBuilder.Owned<Ano>();
        modelBuilder.Owned<Quilometragem>();
        modelBuilder.Owned<PlacaVeiculo>();
        modelBuilder.Owned<Cpf>();
    }
}