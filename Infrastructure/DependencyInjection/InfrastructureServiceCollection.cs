using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Interfaces.Oficina;
using CarStoreManager.Application.Interfaces.Repositories;
using CarStoreManager.Application.Interfaces.Sistema;
using CarStoreManager.Application.Services;
using CarStoreManager.Application.Services.Dashboards;
using CarStoreManager.Application.Services.Concessionaria;
using CarStoreManager.Application.Services.Oficina;
using CarStoreManager.Application.Services.Reports;
using CarStoreManager.Application.Services.Sistema;
using CarStoreManager.Domain.Interfaces.Repositories.Concessionaria;
using CarStoreManager.Domain.Interfaces.Repositories.Integracoes;
using CarStoreManager.Domain.Interfaces.Repositories.Oficina;
using CarStoreManager.Domain.Interfaces.Repositories.Sistema;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Persistence.Repositories;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Infrastructure.Repositories.Concessionaria;
using CarStoreManager.Infrastructure.Repositories.Integracoes;
using CarStoreManager.Infrastructure.Repositories.Sistema;
using CarStoreManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CarStoreManager.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=carstore.db";

        // =========================
        // BANCO DE DADOS (SQLite)
        // =========================
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);

            // Só em dev/local — evita despejar CPF, hash de senha e token do
            // ML em log fora do ambiente de desenvolvimento.
            var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (ambiente != "Production")
            {
                options.EnableSensitiveDataLogging();
            }
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

        // Checklist presets (admin gerencia em Configurações)
        services.AddScoped<IChecklistPresetRepository, ChecklistPresetRepository>();
        services.AddScoped<IChecklistPresetService, ChecklistPresetService>();

        // Pagamento OS (cobrança na recepção)
        services.AddScoped<IPagamentoOrdemServicoRepository, PagamentoOrdemServicoRepository>();
        services.AddScoped<IPagamentoOrdemServicoService, PagamentoOrdemServicoService>();

        // Requisição de peça e alertas
        services.AddScoped<IRequisicaoPecaRepository, RequisicaoPecaRepository>();
        services.AddScoped<IAlertaOSRepository, AlertaOSRepository>();
        services.AddScoped<IRequisicaoPecaService, RequisicaoPecaService>();
        services.AddScoped<IAlertaOSService, AlertaOSService>();

        // =========================
        // CONCESSIONÁRIA
        // =========================
        services.AddScoped<IVeiculoVendaRepository, VeiculoVendaRepository>();
        services.AddScoped<IVeiculoConsignacaoRepository, VeiculoConsignacaoRepository>();
        services.AddScoped<IPropostaVendaRepository, PropostaVendaRepository>();
        services.AddScoped<IVendedorRepository, VendedorRepository>();
        services.AddScoped<IFotoRepository, FotoRepository>();
        services.AddScoped<IFotoService, FotoService>();
        services.AddScoped<IArquivoStorage, ArquivoStorageService>();
        services.AddScoped<IVistoriaRepository, VistoriaRepository>();
        services.AddScoped<ITermoEntregaRepository, TermoEntregaRepository>();

        // Cobrança da proposta (pagamento do veículo)
        services.AddScoped<IPagamentoPropostaRepository, PagamentoPropostaRepository>();
        services.AddScoped<IPagamentoPropostaService, PagamentoPropostaService>();

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
        services.AddScoped<IDespesaRepository, DespesaRepository>();
        services.AddScoped<IDespesaService, DespesaService>();

        // =========================
        // DASHBOARD (admin)
        // =========================
        services.AddScoped<IDashboardService, DashboardService>();

        // =========================
        // RELATÓRIOS (export CSV/XML)
        // =========================
        services.AddScoped<CsvReportFormatter>();
        services.AddScoped<XmlReportFormatter>();
        services.AddScoped<IReportService, ReportService>();

        // =========================
        // INTEGRAÇÕES — MERCADO LIVRE
        // =========================
        services.Configure<MercadoLivreConfig>(configuration.GetSection("MercadoLivre"));
        services.AddScoped<ITokenCriptografiaService, TokenCriptografiaService>();
        services.AddScoped<IAnuncioMercadoLivreRepository, AnuncioMercadoLivreRepository>();
        services.AddScoped<IVendaMercadoLivreRepository, VendaMercadoLivreRepository>();
        services.AddScoped<IConfiguracaoMercadoLivreRepository, ConfiguracaoMercadoLivreRepository>();

        services.AddHttpClient<IMercadoLivreApiClient, MercadoLivreApiClientReal>(c =>
            c.BaseAddress = new Uri("https://api.mercadolibre.com"));

        return services;
    }
}