using System.ComponentModel;
using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Entities;

namespace CarStoreManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // =========================
    // USUÁRIOS
    // =========================
    public DbSet<Usuario> Usuarios { get; set; }

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
    public DbSet<ChecklistOrdemServico> ChecklistItens { get; set; }
    public DbSet<Componente> Componentes { get; set; }

    // =========================
    // CONCESSIONÁRIA
    // =========================
    public DbSet<VeiculoVenda> VeiculosVenda { get; set; }
    public DbSet<Foto> Fotos { get; set; }
    public DbSet<PropostaVenda> PropostasVenda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // TPH — USUÁRIOS
        // =========================
        modelBuilder.Entity<Usuario>()
            .HasDiscriminator<string>("TipoUsuario")
            .HasValue<Admin>("Admin")
            .HasValue<Vendedor>("Vendedor")
            .HasValue<Mecanico>("Mecanico");

        modelBuilder.Entity<Usuario>().HasKey(u => u.Id);

        modelBuilder.Entity<Usuario>()
            .OwnsOne(u => u.Email, e =>
                e.Property("Endereco")
                    .HasColumnName("Email")
                    .IsRequired());

        modelBuilder.Entity<Usuario>()
            .OwnsOne(u => u.Telefone, t =>
                t.Property("Numero")
                    .HasColumnName("Telefone")
                    .IsRequired());

        modelBuilder.Entity<Usuario>()
            .OwnsOne(u => u.Senha, s =>
                s.Property("Hash")
                    .HasColumnName("SenhaHash")
                    .IsRequired());

        modelBuilder.Entity<Usuario>()
            .OwnsOne(d => d.Salario, s =>
                s.Property("Valor")
                    .HasColumnName("Salario")
                    .IsRequired());

        modelBuilder.Entity<Vendedor>()
            .OwnsOne(v => v.DadosFuncionario, d =>
            {
                d.Property("Nivel").HasColumnName("Nivel");
                d.Property("DataContratacao").HasColumnName("DataContratacao");
            });

        modelBuilder.Entity<Mecanico>()
            .OwnsOne(m => m.DadosFuncionario, d =>
            {
                d.Property("Nivel").HasColumnName("Nivel");
                d.Property("DataContratacao").HasColumnName("DataContratacao");
            });

        modelBuilder.Entity<Mecanico>()
            .Property(m => m.TrabalhosAtivos)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(Guid.Parse)
                      .ToList())
            .HasColumnName("TrabalhosAtivos");

        // =========================
        // CLIENTE
        // =========================
        modelBuilder.Entity<Cliente>().HasKey(c => c.Id);

        modelBuilder.Entity<Cliente>()
            .OwnsOne(c => c.Email, e =>
                e.Property("Endereco")
                    .HasColumnName("Email")
                    .IsRequired());

        modelBuilder.Entity<Cliente>()
            .OwnsOne(c => c.Telefone, t =>
                t.Property("Numero")
                    .HasColumnName("Telefone")
                    .IsRequired());

        modelBuilder.Entity<Cliente>()
            .OwnsOne(c => c.Cpf, cpf =>
                cpf.Property("Numero")
                    .HasColumnName("CPF")
                    .IsRequired());

        // =========================
        // VEICULO CLIENTE
        // =========================
        modelBuilder.Entity<VeiculoCliente>().HasKey(v => v.Id);

        modelBuilder.Entity<VeiculoCliente>()
            .OwnsOne(v => v.Ano, a =>
                a.Property("Valor").HasColumnName("Ano"));

        modelBuilder.Entity<VeiculoCliente>()
            .HasMany(v => v.HistoricoServicos)
            .WithOne()
            .HasForeignKey("VeiculoClienteId")
            .OnDelete(DeleteBehavior.SetNull);

        // =========================
        // ORDEM DE SERVIÇO
        // =========================
        modelBuilder.Entity<OrdemServico>().HasKey(o => o.Id);

        modelBuilder.Entity<OrdemServico>()
            .HasIndex(o => o.NumeroPublico)
            .IsUnique();

        modelBuilder.Entity<OrdemServico>()
            .OwnsOne(o => o.CustoServico, d =>
                d.Property("Valor").HasColumnName("CustoServico"));

        modelBuilder.Entity<OrdemServico>()
            .OwnsOne(o => o.ValorTotal, d =>
                d.Property("Valor").HasColumnName("ValorTotal"));

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
        // ITEM ORDEM DE SERVIÇO
        // =========================
        modelBuilder.Entity<ItemOrdemServico>().HasKey(i => i.Id);

        modelBuilder.Entity<ItemOrdemServico>()
            .OwnsOne(i => i.ValorUnitario, d =>
                d.Property("Valor").HasColumnName("ValorUnitario"));

        modelBuilder.Entity<ItemOrdemServico>()
            .OwnsOne(i => i.ValorTotal, d =>
                d.Property("Valor").HasColumnName("ValorTotalItem"));

        // =========================
        // CHECKLIST ITEM
        // =========================
        modelBuilder.Entity<ChecklistOrdemServico>().HasKey(c => c.Id);

        // =========================
        // COMPONENTE
        // =========================
        modelBuilder.Entity<Componente>().HasKey(c => c.Id);

        modelBuilder.Entity<Componente>()
            .OwnsOne(c => c.Valor, d =>
                d.Property("Valor").HasColumnName("ValorUnitario"));

        // =========================
        // VEICULO VENDA
        // =========================
        modelBuilder.Entity<VeiculoVenda>().HasKey(v => v.Id);

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Ano, a =>
                a.Property("Valor").HasColumnName("Ano"));

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Quilometragem, q =>
                q.Property("Valor").HasColumnName("Quilometragem"));

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Placa, p =>
                p.Property("Valor").HasColumnName("Placa"));

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Valor, d =>
                d.Property("Valor").HasColumnName("Valor"));

        modelBuilder.Entity<VeiculoVenda>()
            .HasMany(v => v.Fotos)
            .WithOne()
            .HasForeignKey(f => f.VeiculoVendaId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================
        // FOTO VEICULO
        // =========================
        modelBuilder.Entity<FotoVeiculo>().HasKey(f => f.Id);

        // =========================
        // PROPOSTA VENDA
        // =========================
        modelBuilder.Entity<PropostaVenda>(entity =>
        {
            entity.HasKey(p => p.Id);

            // ===== DIRETO NA ENTIDADE =====

            entity.OwnsOne(p => p.ValorBase, vo =>
            {
                vo.Property("Valor").HasColumnName("ValorBase").HasPrecision(18,2);
            });

            entity.OwnsOne(p => p.ValorFinal, vo =>
            {
                vo.Property("Valor").HasColumnName("ValorFinal").HasPrecision(18,2);
            });

            entity.OwnsOne(p => p.Entrada, vo =>
            {
                vo.Property("Valor").HasColumnName("Entrada").HasPrecision(18,2);
            });

            entity.OwnsOne(p => p.Desconto, vo =>
            {
                vo.Property("Valor").HasColumnName("DescontoPercentual");
            });

            // ===== FINANCIAMENTO =====
        });
    }
}