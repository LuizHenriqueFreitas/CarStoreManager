using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Oficina;
using CarStoreManager.Domain.Entities.Sistema;
using CarStoreManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<EstoqueComponente> EstoqueComponentes { get; set; }
    public DbSet<PagamentoOrdemServico> PagamentosOrdemServico { get; set; }
    public DbSet<RequisicaoPecaOS> RequisicoesPeca { get; set; }
    public DbSet<AlertaOS> AlertasOS { get; set; }
    public DbSet<NotaFiscalVendaOS> NotasFiscaisVendaOS { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; }
    public DbSet<LoteComponente> LotesComponente { get; set; }

    // =========================
    // CONCESSIONÁRIA
    // =========================
    public DbSet<VeiculoVenda> VeiculosVenda { get; set; }
    public DbSet<Foto> Fotos { get; set; }
    public DbSet<PropostaVenda> PropostasVenda { get; set; }
    public DbSet<Vistoria> Vistorias { get; set; }
    public DbSet<TermoEntrega> TermosEntrega { get; set; }
    public DbSet<PagamentoProposta> PagamentosProposta { get; set; }

    // =========================
    // SISTEMA
    // =========================
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; }

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
            .HasValue<Mecanico>("Mecanico")
            .HasValue<Recepcionista>("Recepcionista");

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

        modelBuilder.Entity<Recepcionista>()
            .OwnsOne(r => r.DadosFuncionario, d =>
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
            .OwnsOne(v => v.Placa, p =>
                p.Property("Valor").HasColumnName("Placa").IsRequired());

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
        modelBuilder.Entity<ChecklistOrdemServico>().Property(c => c.Titulo).IsRequired(false);

        // =========================
        // COMPONENTE
        // =========================
        modelBuilder.Entity<Componente>().HasKey(c => c.Id);
        // SKUInterno, PartNumber, CodigoOEM, CodigoBarras, NCM, CEST
        // são strings simples — EF mapeia automaticamente.

        // Equivalências bidirecionais — duas navigations para a mesma tabela.
        modelBuilder.Entity<ComponenteEquivalente>().HasKey(e => e.Id);

        modelBuilder.Entity<Componente>()
            .HasMany(c => c.EquivalenciasOriginais)
            .WithOne(e => e.ComponenteOriginal)
            .HasForeignKey(e => e.ComponenteOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Componente>()
            .HasMany(c => c.EquivalenciasRelacionadas)
            .WithOne(e => e.ComponenteEquivalenteRelacionado)
            .HasForeignKey(e => e.ComponenteEquivalenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // REQUISIÇÃO DE PEÇA / ALERTA OS
        // =========================
        modelBuilder.Entity<RequisicaoPecaOS>().HasKey(r => r.Id);
        modelBuilder.Entity<RequisicaoPecaOS>().HasIndex(r => r.OrdemServicoId);
        modelBuilder.Entity<RequisicaoPecaOS>().HasIndex(r => r.Status);

        modelBuilder.Entity<AlertaOS>().HasKey(a => a.Id);
        modelBuilder.Entity<AlertaOS>().HasIndex(a => a.OrdemServicoId);
        modelBuilder.Entity<AlertaOS>().HasIndex(a => a.Status);

        // =========================
        // PAGAMENTO ORDEM SERVICO
        // =========================
        modelBuilder.Entity<PagamentoOrdemServico>().HasKey(p => p.Id);
        modelBuilder.Entity<PagamentoOrdemServico>()
            .HasIndex(p => p.OrdemServicoId);
        modelBuilder.Entity<PagamentoOrdemServico>()
            .OwnsOne(p => p.Valor, vo =>
                vo.Property("Valor").HasColumnName("Valor").HasPrecision(18, 2));

        // =========================
        // FORNECEDOR
        // =========================
        modelBuilder.Entity<Fornecedor>().HasKey(f => f.Id);

        modelBuilder.Entity<Fornecedor>()
            .OwnsOne(f => f.Cnpj, c =>
                c.Property(x => x.Numero).HasColumnName("Cnpj").IsRequired());

        modelBuilder.Entity<Fornecedor>()
            .OwnsOne(f => f.Email, e =>
                e.Property("Endereco").HasColumnName("Email").IsRequired());

        modelBuilder.Entity<Fornecedor>()
            .OwnsOne(f => f.Telefone, t =>
                t.Property("Numero").HasColumnName("Telefone").IsRequired());

        // =========================
        // NOTA FISCAL (entrada)
        // =========================
        modelBuilder.Entity<NotaFiscal>().HasKey(n => n.Id);
        modelBuilder.Entity<NotaFiscal>()
            .HasIndex(n => n.ChaveAcesso).IsUnique();
        modelBuilder.Entity<NotaFiscal>()
            .Property(n => n.ValorProdutos).HasPrecision(18, 2);
        modelBuilder.Entity<NotaFiscal>()
            .Property(n => n.ValorImpostos).HasPrecision(18, 2);
        modelBuilder.Entity<NotaFiscal>()
            .Property(n => n.ValorTotal).HasPrecision(18, 2);
        modelBuilder.Entity<NotaFiscal>()
            .HasOne(n => n.Fornecedor)
            .WithMany()
            .HasForeignKey(n => n.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<NotaFiscal>()
            .HasMany(n => n.Itens)
            .WithOne(i => i.NotaFiscal)
            .HasForeignKey(i => i.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemNotaFiscal>().HasKey(i => i.Id);
        modelBuilder.Entity<ItemNotaFiscal>()
            .Property(i => i.ValorUnitario).HasPrecision(18, 4);
        modelBuilder.Entity<ItemNotaFiscal>()
            .Property(i => i.ValorTotal).HasPrecision(18, 2);
        modelBuilder.Entity<ItemNotaFiscal>()
            .Property(i => i.AliquotaIcms).HasPrecision(5, 2);
        modelBuilder.Entity<ItemNotaFiscal>()
            .Property(i => i.ValorIcms).HasPrecision(18, 2);
        modelBuilder.Entity<ItemNotaFiscal>()
            .HasOne(i => i.Componente)
            .WithMany()
            .HasForeignKey(i => i.ComponenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // LOTE COMPONENTE
        // =========================
        modelBuilder.Entity<LoteComponente>().HasKey(l => l.Id);
        modelBuilder.Entity<LoteComponente>()
            .HasOne(l => l.Componente)
            .WithMany(c => c.Lotes)
            .HasForeignKey(l => l.ComponenteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LoteComponente>()
            .HasOne(l => l.Fornecedor)
            .WithMany()
            .HasForeignKey(l => l.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LoteComponente>()
            .HasOne(l => l.NotaFiscal)
            .WithMany()
            .HasForeignKey(l => l.NotaFiscalId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LoteComponente>()
            .HasIndex(l => new { l.ComponenteId, l.NumeroLote });

        // =========================
        // ESTOQUE COMPONENTE
        // =========================
        modelBuilder.Entity<EstoqueComponente>().HasKey(e => e.Id);
        modelBuilder.Entity<EstoqueComponente>()
            .HasOne(e => e.Componente)
            .WithMany()
            .HasForeignKey(e => e.PecaId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<EstoqueComponente>()
            .HasIndex(e => e.PecaId)
            .IsUnique();

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

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Renavam, r =>
                r.Property(x => x.Numero).HasColumnName("Renavam").IsRequired());

        // =========================
        // FOTO
        // =========================
        modelBuilder.Entity<Foto>().HasKey(f => f.Id);
        modelBuilder.Entity<Foto>().HasIndex(f => new { f.EntidadeTipo, f.EntidadeId });

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
            entity.OwnsOne(p => p.ValorParcela, vo =>
            {
                vo.Property("Valor").HasColumnName("ValorParcela").HasPrecision(18, 2);
            });
            entity.Property(p => p.TaxaJurosMensal).HasPrecision(7, 4);
        });

        // =========================
        // VISTORIA
        // =========================
        modelBuilder.Entity<Vistoria>().HasKey(v => v.Id);
        modelBuilder.Entity<Vistoria>()
            .HasIndex(v => v.PropostaVendaId);

        // =========================
        // TERMO ENTREGA
        // =========================
        modelBuilder.Entity<TermoEntrega>().HasKey(t => t.Id);
        modelBuilder.Entity<TermoEntrega>()
            .HasIndex(t => t.PropostaVendaId).IsUnique();
        modelBuilder.Entity<TermoEntrega>()
            .HasIndex(t => t.TokenAssinatura);

        // =========================
        // PAGAMENTO PROPOSTA (cobrança do veículo)
        // =========================
        modelBuilder.Entity<PagamentoProposta>().HasKey(p => p.Id);
        modelBuilder.Entity<PagamentoProposta>()
            .HasIndex(p => p.PropostaVendaId);
        modelBuilder.Entity<PagamentoProposta>()
            .OwnsOne(p => p.Valor, vo =>
                vo.Property("Valor").HasColumnName("Valor").HasPrecision(18, 2));

        // =========================
        // CONFIGURAÇÃO SISTEMA (singleton)
        // =========================
        modelBuilder.Entity<ConfiguracaoSistema>().HasKey(c => c.Id);
        modelBuilder.Entity<ConfiguracaoSistema>()
            .Property(c => c.MargemPadraoGlobalPct).HasPrecision(7, 4);

        // === Precificação de componentes ===
        modelBuilder.Entity<Componente>()
            .Property(c => c.CustoUnitario).HasPrecision(18, 4);
        modelBuilder.Entity<Componente>()
            .Property(c => c.MargemLucroPct).HasPrecision(7, 4);
        modelBuilder.Entity<Componente>()
            .Property(c => c.ValorVenda).HasPrecision(18, 2);

        // === NF de venda da OS ===
        modelBuilder.Entity<NotaFiscalVendaOS>().HasKey(n => n.Id);
        modelBuilder.Entity<NotaFiscalVendaOS>().HasIndex(n => n.OrdemServicoId).IsUnique();
        modelBuilder.Entity<NotaFiscalVendaOS>().HasIndex(n => n.Numero).IsUnique();
        modelBuilder.Entity<NotaFiscalVendaOS>()
            .OwnsOne(n => n.ValorServico, vo =>
                vo.Property("Valor").HasColumnName("ValorServico").HasPrecision(18, 2));
        modelBuilder.Entity<NotaFiscalVendaOS>()
            .OwnsOne(n => n.ValorPecas, vo =>
                vo.Property("Valor").HasColumnName("ValorPecas").HasPrecision(18, 2));
        modelBuilder.Entity<NotaFiscalVendaOS>()
            .OwnsOne(n => n.ValorTotal, vo =>
                vo.Property("Valor").HasColumnName("ValorTotal").HasPrecision(18, 2));
    }
}