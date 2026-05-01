using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Data;
using CarStoreManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
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
        });

        // =========================
        // OFICINA
        // =========================
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IComponenteRepository, ComponenteRepository>();
        services.AddScoped<IMecanicoRepository, MecanicoRepository>();
        services.AddScoped<IVeiculoClienteRepository, VeiculoClienteRepository>();

        // =========================
        // CONCESSIONÁRIA
        // =========================
        services.AddScoped<IVeiculoVendaRepository, VeiculoVendaRepository>();
        services.AddScoped<IPropostaVendaRepository, PropostaVendaRepository>();
        services.AddScoped<IVendedorRepository, VendedorRepository>();

        // =========================
        // CLIENTES
        // =========================
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IClienteService, ClienteService>();

        // =========================
        // USUÁRIOS (GENÉRICO SE NECESSÁRIO)
        // =========================
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}