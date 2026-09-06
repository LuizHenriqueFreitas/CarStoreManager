using CarStoreManager.Domain.Entities;
using CarStoreManager.Domain.Entities.Concessionaria;
using CarStoreManager.Domain.Entities.Integracoes;
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
    public DbSet<Endereco> Enderecos { get; set; }
    public DbSet<VeiculoCliente> VeiculosCliente { get; set; }

    // =========================
    // OFICINA
    // =========================
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<ItemOrdemServico> ItensOrdemServico { get; set; }
    public DbSet<ChecklistOrdemServico> ChecklistItens { get; set; }
    public DbSet<ChecklistPreset> ChecklistPresets { get; set; }
    public DbSet<ChecklistPresetItem> ChecklistPresetItens { get; set; }
    public DbSet<Componente> Componentes { get; set; }
    public DbSet<EstoqueComponente> EstoqueComponentes { get; set; }
    public DbSet<PagamentoOrdemServico> PagamentosOrdemServico { get; set; }
    public DbSet<RequisicaoPecaOS> RequisicoesPeca { get; set; }
    public DbSet<AlertaOS> AlertasOS { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }

    // =========================
    // CONCESSIONÁRIA
    // =========================
    public DbSet<VeiculoVenda> VeiculosVenda { get; set; }
    public DbSet<VeiculoConsignacao> VeiculosConsignacao { get; set; }
    public DbSet<HistoricoConsignacao> HistoricosConsignacao { get; set; }
    public DbSet<Foto> Fotos { get; set; }
    public DbSet<PropostaVenda> PropostasVenda { get; set; }
    public DbSet<Vistoria> Vistorias { get; set; }
    public DbSet<TermoEntrega> TermosEntrega { get; set; }
    public DbSet<PagamentoProposta> PagamentosProposta { get; set; }

    // =========================
    // SISTEMA
    // =========================
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema { get; set; }
    public DbSet<TemplateDocumento> TemplatesDocumento { get; set; }
    public DbSet<Despesa> Despesas { get; set; }

    // =========================
    // INTEGRAÇÕES — MERCADO LIVRE
    // =========================
    public DbSet<AnuncioMercadoLivre> AnunciosMercadoLivre { get; set; }
    public DbSet<VendaMercadoLivre> VendasMercadoLivre { get; set; }
    public DbSet<ConfiguracaoMercadoLivre> ConfiguracoesMercadoLivre { get; set; }

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
            .HasValue<Recepcionista>("Recepcionista")
            .HasValue<ChefeOficina>("ChefeOficina")
            .HasValue<GerenteVendas>("GerenteVendas");

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

        modelBuilder.Entity<ChefeOficina>()
            .OwnsOne(c => c.DadosFuncionario, d =>
            {
                d.Property("Nivel").HasColumnName("Nivel");
                d.Property("DataContratacao").HasColumnName("DataContratacao");
            });

        modelBuilder.Entity<GerenteVendas>()
            .OwnsOne(g => g.DadosFuncionario, d =>
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
        // ENDERECO (tabela própria — Cliente guarda só a FK EnderecoId)
        // =========================
        modelBuilder.Entity<Endereco>().HasKey(e => e.Id);
        modelBuilder.Entity<Endereco>().Property(e => e.Logradouro).IsRequired();
        modelBuilder.Entity<Endereco>().Property(e => e.Numero).IsRequired();
        modelBuilder.Entity<Endereco>().Property(e => e.Bairro).IsRequired();
        modelBuilder.Entity<Endereco>().Property(e => e.Cidade).IsRequired();
        modelBuilder.Entity<Endereco>().Property(e => e.Uf).HasMaxLength(2).IsRequired();
        modelBuilder.Entity<Endereco>().Property(e => e.Cep).HasMaxLength(8).IsRequired();

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Endereco)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.EnderecoId)
            .IsRequired();

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
        // CHECKLIST PRESET (editável pelo admin)
        // =========================
        modelBuilder.Entity<ChecklistPreset>().HasKey(p => p.Id);
        modelBuilder.Entity<ChecklistPreset>().Property(p => p.Nome).IsRequired();
        modelBuilder.Entity<ChecklistPreset>()
            .HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.ChecklistPresetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChecklistPresetItem>().HasKey(i => i.Id);
        modelBuilder.Entity<ChecklistPresetItem>().Property(i => i.Descricao).IsRequired();

        // =========================
        // TEMPLATE DE DOCUMENTO (editável pelo admin — contratos/termos genéricos)
        // =========================
        modelBuilder.Entity<TemplateDocumento>().HasKey(t => t.Id);
        modelBuilder.Entity<TemplateDocumento>().Property(t => t.Nome).IsRequired();
        modelBuilder.Entity<TemplateDocumento>().Property(t => t.Conteudo).IsRequired();

        // =========================
        // COMPONENTE
        // =========================
        modelBuilder.Entity<Componente>().HasKey(c => c.Id);
        // SKUInterno, PartNumber, CodigoOEM, CodigoBarras, NCM, CEST
        // são strings simples — EF mapeia automaticamente.

        // Sem navigation property (o service resolve o nome do fornecedor via
        // repositório, não via Include) — só a FK mesmo. Restrict: excluir um
        // fornecedor com componentes vinculados deve falhar (o admin desativa
        // em vez de excluir), não apagar em cascata nem deixar componente órfão.
        modelBuilder.Entity<Componente>()
            .HasOne<Fornecedor>()
            .WithMany()
            .HasForeignKey(c => c.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

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
        // FORNECEDOR
        // =========================
        modelBuilder.Entity<Fornecedor>().HasKey(f => f.Id);

        modelBuilder.Entity<Fornecedor>()
            .OwnsOne(f => f.Cnpj, c =>
                c.Property(x => x.Numero)
                    .HasColumnName("CNPJ")
                    .IsRequired());

        modelBuilder.Entity<Fornecedor>()
            .HasOne(f => f.Endereco)
            .WithOne()
            .HasForeignKey<Fornecedor>(f => f.EnderecoId)
            .IsRequired(false);

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
            .OwnsOne(v => v.ValorAquisicao, d =>
                d.Property("Valor").HasColumnName("ValorAquisicao"));

        modelBuilder.Entity<VeiculoVenda>()
            .HasMany(v => v.Fotos)
            .WithOne()
            .HasForeignKey(f => f.VeiculoVendaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VeiculoVenda>()
            .OwnsOne(v => v.Renavam, r =>
                r.Property(x => x.Numero).HasColumnName("Renavam").IsRequired());

        // =========================
        // VEICULO CONSIGNACAO
        // =========================
        modelBuilder.Entity<VeiculoConsignacao>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.OwnsOne(v => v.Ano, a => a.Property("Valor").HasColumnName("Ano"));
            entity.OwnsOne(v => v.Quilometragem, q => q.Property("Valor").HasColumnName("Quilometragem"));
            entity.OwnsOne(v => v.Placa, p => p.Property("Valor").HasColumnName("Placa"));
            entity.OwnsOne(v => v.Renavam, r => r.Property(x => x.Numero).HasColumnName("Renavam").IsRequired());

            entity.OwnsOne(v => v.Comissao, c =>
            {
                c.Property(x => x.Tipo).HasColumnName("ComissaoTipo").HasConversion<int>();

                c.OwnsOne(x => x.ValorVendaEsperado, vo =>
                    vo.Property("Valor").HasColumnName("ValorVendaEsperado").HasPrecision(18, 2).IsRequired());

                // Sem .IsRequired(): coluna aceita NULL — mutuamente exclusivo com PorcentagemProprietario.
                c.OwnsOne(x => x.ValorFixoProprietario, vo =>
                    vo.Property("Valor").HasColumnName("ComissaoValorFixoProprietario").HasPrecision(18, 2));

                c.OwnsOne(x => x.PorcentagemProprietario, vo =>
                    vo.Property("Valor").HasColumnName("ComissaoPorcentagemProprietario"));
            });

            entity.HasMany(v => v.Historico)
                .WithOne()
                .HasForeignKey(h => h.VeiculoConsignacaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HistoricoConsignacao>().HasKey(h => h.Id);
        modelBuilder.Entity<HistoricoConsignacao>().HasIndex(h => h.VeiculoConsignacaoId);
        // Sem isso, o EF usa a convenção padrão de chave Guid (ValueGeneratedOnAdd)
        // pra decidir Added vs Modified pelo valor do Id — como o Id já vem
        // preenchido pelo construtor (Entity.Id = Guid.NewGuid()), toda vez que
        // um evento novo é adicionado à coleção Historico de um VeiculoConsignacao
        // JÁ RASTREADO (carregado via Include), o EF classifica o item novo como
        // Modified em vez de Added — o UPDATE gerado pra uma linha que não existe
        // ainda afeta 0 linhas e SaveChanges lança DbUpdateConcurrencyException.
        // ValueGeneratedNever() faz o EF confiar só no estado de rastreamento
        // (nunca visto antes = Added), que é o comportamento correto pra chaves
        // sempre geradas pela aplicação.
        modelBuilder.Entity<HistoricoConsignacao>().Property(h => h.Id).ValueGeneratedNever();

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
        modelBuilder.Entity<ConfiguracaoSistema>()
            .Property(c => c.PercentualEntradaMinima).HasPrecision(5, 2);

        // =========================
        // DESPESA (planilha mensal do admin)
        // =========================
        modelBuilder.Entity<Despesa>().HasKey(d => d.Id);
        modelBuilder.Entity<Despesa>().Property(d => d.Nome).IsRequired();
        modelBuilder.Entity<Despesa>()
            .Property(d => d.Setor)
            .HasConversion<string>()
            .HasDefaultValue(CarStoreManager.Domain.Enums.SetorDespesa.Geral);
        modelBuilder.Entity<Despesa>()
            .Property(d => d.Tipo)
            .HasConversion<string>()
            .HasDefaultValue(CarStoreManager.Domain.Enums.TipoDespesa.Outros);
        modelBuilder.Entity<Despesa>()
            .OwnsOne(d => d.Valor, vo =>
                vo.Property("Valor").HasColumnName("Valor").HasPrecision(18, 2));

        // === Precificação de componentes ===
        modelBuilder.Entity<Componente>()
            .Property(c => c.CustoUnitario).HasPrecision(18, 4);
        modelBuilder.Entity<Componente>()
            .Property(c => c.MargemLucroPct).HasPrecision(7, 4);
        modelBuilder.Entity<Componente>()
            .Property(c => c.ValorVenda).HasPrecision(18, 2);

        // =========================
        // INTEGRAÇÕES — MERCADO LIVRE
        // =========================
        modelBuilder.Entity<AnuncioMercadoLivre>().HasKey(a => a.Id);
        modelBuilder.Entity<AnuncioMercadoLivre>()
            .HasIndex(a => new { a.EntidadeTipo, a.EntidadeId }).IsUnique();
        modelBuilder.Entity<AnuncioMercadoLivre>().HasIndex(a => a.ItemIdML);
        modelBuilder.Entity<AnuncioMercadoLivre>()
            .Property(a => a.UltimoPrecoSincronizado).HasPrecision(18, 2);

        modelBuilder.Entity<VendaMercadoLivre>().HasKey(v => v.Id);
        modelBuilder.Entity<VendaMercadoLivre>()
            // Idempotência garantida também no nível do banco — não só na lógica de aplicação.
            .HasIndex(v => new { v.IdPedidoPlataforma, v.IdItemPlataforma }).IsUnique();
        modelBuilder.Entity<VendaMercadoLivre>()
            .Property(v => v.PrecoUnitario).HasPrecision(18, 2);

        modelBuilder.Entity<ConfiguracaoMercadoLivre>().HasKey(c => c.Id);
    }
}