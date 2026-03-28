using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;
using CarStoreManager.Infrastructure.Services;

namespace CarStoreManager.Web.Extensions;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Shared
        services.AddScoped<IClienteService, ClienteService>();

        // Concessionaria
        services.AddScoped<IVeiculoVendaService, VeiculoVendaService>();
        services.AddScoped<IVendedorService, VendedorService>();
        services.AddScoped<IPropostaVendaService, PropostaVendaService>();

        // Oficina
        services.AddScoped<IVeiculoClienteService, VeiculoClienteService>();
        services.AddScoped<IMecanicoService, MecanicoService>();
        services.AddScoped<IComponenteService, ComponenteService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();

        // Auth
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}