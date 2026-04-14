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
        // HERANÇA (TPH - Table per Hierarchy) para Usuario
        // =========================
        modelBuilder.Entity<Usuario>()
            .HasDiscriminator<string>("TipoUsuario")
            .HasValue<Usuario>("Usuario")
            .HasValue<Admin>("Admin")
            .HasValue<Vendedor>("Vendedor")
            .HasValue<Mecanico>("Mecanico");

        // =========================
        // CONFIGURAÇÕES DA CLASSE BASE USUARIO
        // (Garante que Email, Telefone etc. sejam owned types para todas as subclasses)
        // =========================
        modelBuilder.Entity<Usuario>(entity =>
        {
            // Email (ValueObject)
            entity.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Endereco)        // Ajuste o nome da propriedade conforme sua classe Email
                      .HasColumnName("Email")
                      .HasMaxLength(150)
                      .IsRequired();
            });

            // Telefone (ValueObject)
            entity.OwnsOne(u => u.Telefone, telefone =>
            {
                telefone.Property(t => t.Numero)     // Ajuste conforme sua classe Telefone
                        .HasColumnName("Telefone")
                        .HasMaxLength(20);
            });

            // Outros owned types comuns a todos os usuários, se houver (ex: Endereco)
        });

        // =========================
        // CLIENTE (especificidades)
        // =========================
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.OwnsOne(c => c.Email);      // Email já configurado em Usuario? Se Cliente herdar de Usuario, remova
            entity.OwnsOne(c => c.Telefone);
            entity.OwnsOne(c => c.CPF, cpf =>
            {
                cpf.Property(p => p.Numero)
                    .HasColumnName("Cpf")
                    .HasMaxLength(14);
            });
        });

        // =========================
        // VEICULO CLIENTE
        // =========================
        modelBuilder.Entity<VeiculoCliente>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.OwnsOne(v => v.Ano, ano =>
            {
                ano.Property(a => a.Valor).HasColumnName("Ano");
            });
            entity.HasMany(v => v.HistoricoServicos)
                  .WithOne()
                  .HasForeignKey("VeiculoId");
        });

        // =========================
        // MECANICO (especificidades)
        // =========================
        modelBuilder.Entity<Mecanico>(entity =>
        {
            entity.OwnsOne(m => m.Email);      // Se herdar de Usuario, pode ser redundante, mas não quebra
            entity.OwnsOne(m => m.Telefone);
            entity.OwnsOne(m => m.DadosFuncionario, dados =>
            {
                dados.Property(d => d.DataContratacao).HasColumnName("DataContratacao");
                dados.Property(d => d.Nivel).HasColumnName("Nivel");
                // ... outros campos
            });
        });

        // =========================
        // VENDEDOR (especificidades)
        // =========================
        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.OwnsOne(v => v.Email);
            entity.OwnsOne(v => v.Telefone);
            entity.OwnsOne(v => v.DadosFuncionario);
        });

        // =========================
        // ORDEM SERVICO
        // =========================
        modelBuilder.Entity<OrdemServico>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.OwnsOne(o => o.CustoServico, custo => { custo.Property(c => c.Valor).HasColumnName("CustoServico"); });
            entity.OwnsOne(o => o.ValorTotal, total => { total.Property(t => t.Valor).HasColumnName("ValorTotal"); });
            entity.HasMany(o => o.Itens)
                  .WithOne()
                  .HasForeignKey(i => i.OrdemServicoId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(o => o.Checklist)
                  .WithOne()
                  .HasForeignKey(c => c.OrdemServicoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // ITEM ORDEM SERVICO
        // =========================
        modelBuilder.Entity<ItemOrdemServico>(entity =>
        {
            entity.OwnsOne(i => i.ValorUnitario, vu => { vu.Property(v => v.Valor).HasColumnName("ValorUnitario"); });
            entity.OwnsOne(i => i.ValorTotal, vt => { vt.Property(v => v.Valor).HasColumnName("ValorTotalItem"); });
        });

        // =========================
        // COMPONENTE
        // =========================
        modelBuilder.Entity<Componente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.OwnsOne(c => c.Valor, valor => { valor.Property(v => v.Valor).HasColumnName("ValorComponente"); });
        });

        // =========================
        // VEICULO VENDA
        // =========================
        modelBuilder.Entity<VeiculoVenda>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.OwnsOne(v => v.Ano, ano => { ano.Property(a => a.Valor).HasColumnName("Ano"); });
            entity.OwnsOne(v => v.Quilometragem, km => { km.Property(k => k.Valor).HasColumnName("Quilometragem"); });
            entity.OwnsOne(v => v.Placa, placa => { placa.Property(p => p.Valor).HasColumnName("Placa"); });
            entity.OwnsOne(v => v.Valor, valor => { valor.Property(v => v.Valor).HasColumnName("ValorVeiculo"); });
            entity.HasMany(v => v.Fotos)
                  .WithOne()
                  .HasForeignKey(f => f.VeiculoVendaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // PROPOSTA VENDA
        // =========================
        modelBuilder.Entity<PropostaVenda>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.OwnsOne(p => p.ValorBase, vb => vb.Property(v => v.Valor).HasColumnName("ValorBase"));
            entity.OwnsOne(p => p.ValorFinal, vf => vf.Property(v => v.Valor).HasColumnName("ValorFinal"));
            entity.OwnsOne(p => p.Desconto, desc => desc.Property(d => d.Valor).HasColumnName("Desconto"));

            // Configuração de Financiamento (Value Object)
            entity.OwnsOne(p => p.Financiamento, financiamento =>
            {
                // Se Financiamento possuir outras propriedades, mapeie aqui
                // financiamento.Property(f => f.NumeroParcelas).HasColumnName("Financiamento_Parcelas");

                // Dentro de Financiamento, Entrada é do tipo Dinheiro (outro Value Object)
                financiamento.OwnsOne(f => f.Entrada, entrada =>
                {
                    entrada.Property(d => d.Valor)
                        .HasColumnName("Financiamento_Entrada")
                        .HasColumnType("decimal(18,2)");

                    // Se Dinheiro tiver TrocoPara, mapeie também
                    // entrada.Property(d => d.TrocoPara).HasColumnName("Financiamento_Troco");
                });
            });

            // Entrada da Proposta (caso PropostaVenda tenha uma propriedade Entrada separada do Financiamento)
            entity.OwnsOne(p => p.Entrada, entrada =>
            {
                entrada.Property(d => d.Valor)
                    .HasColumnName("ValorEntrada")
                    .HasColumnType("decimal(18,2)");
            });
        });
    }
}