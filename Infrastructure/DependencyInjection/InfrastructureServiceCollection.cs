using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Repositories;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Application.Services;
using CarStoreManager.Application.Services.Oficina;
using CarStoreManager.Application.Services.Oficina.NotaFiscal;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Persistence.Repositories;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Infrastructure.Repositories.Concessionaria;
using CarStoreManager.Infrastructure.Repositories.Sistema;
using CarStoreManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // =========================
        // BANCO DE DADOS
        // =========================
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);

            // opcional (debug)
            options.EnableSensitiveDataLogging();

            // Suprime o warning de "PendingModelChangesWarning" enquanto a migration
            // não é regenerada — o EnsureCreated/MigrateAsync continua aplicando o schema.
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // =========================
        // OFICINA
        // =========================
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IComponenteRepository, ComponenteRepository>();
        services.AddScoped<IEstoqueRepository, EstoqueRepository>();
        services.AddScoped<IEstoqueService, EstoqueService>();
        services.AddScoped<IMecanicoRepository, MecanicoRepository>();
        services.AddScoped<IVeiculoClienteRepository, VeiculoClienteRepository>();

        // NF-e (entrada)
        services.AddScoped<IFornecedorRepository, FornecedorRepository>();
        services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
        services.AddScoped<ILoteComponenteRepository, LoteComponenteRepository>();
        services.AddScoped<INotaFiscalEntradaService, NotaFiscalEntradaService>();

        // Pagamento OS (cobrança na recepção)
        services.AddScoped<IPagamentoOrdemServicoRepository, PagamentoOrdemServicoRepository>();
        services.AddScoped<IPagamentoOrdemServicoService, PagamentoOrdemServicoService>();

        // =========================
        // CONCESSIONÁRIA
        // =========================
        services.AddScoped<IVeiculoVendaRepository, VeiculoVendaRepository>();
        services.AddScoped<IPropostaVendaRepository, PropostaVendaRepository>();
        services.AddScoped<IVendedorRepository, VendedorRepository>();
        services.AddScoped<IFotoRepository, FotoRepository>();
        services.AddScoped<IFotoService, FotoService>();
        services.AddScoped<IArquivoStorage, ArquivoStorageService>();
        services.AddScoped<IVistoriaRepository, VistoriaRepository>();
        services.AddScoped<ITermoEntregaRepository, TermoEntregaRepository>();

        // =========================
        // CLIENTES
        // =========================
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IClienteService, ClienteService>();

        // =========================
        // USUÁRIOS (GENÉRICO SE NECESSÁRIO)
        // =========================
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // =========================
        // SISTEMA
        // =========================
        services.AddScoped<IConfiguracaoSistemaRepository, ConfiguracaoSistemaRepository>();
        services.AddScoped<IConfiguracaoSistemaService, ConfiguracaoSistemaService>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}