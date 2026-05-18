using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;
using CarStoreManager.Application.Services.Concessionaria;
using CarStoreManager.Application.Services.Oficina;
using CarStoreManager.Application.Services.Shared;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Infrastructure.Services;

namespace CarStoreManager.Web.Extensions;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Shared
        services.AddScoped<IClienteService, ClienteService>();

        // Concessionaria
        services.AddScoped<IVeiculoVendaService, VeiculoVendaService>();
        services.AddScoped<IVendedorService, VendedorService>();
        services.AddScoped<IGerenteVendasService, GerenteVendasService>();
        services.AddScoped<IPropostaVendaService, PropostaVendaService>();

        // Oficina
        services.AddScoped<IVeiculoClienteService, VeiculoClienteService>();
        services.AddScoped<IMecanicoService, MecanicoService>();
        services.AddScoped<IChefeOficinaService, ChefeOficinaService>();
        services.AddScoped<IComponenteService, ComponenteService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();

        // Recepcionista (oficina, mas vive no namespace base)
        services.AddScoped<IRecepcionistaService, RecepcionistaService>();

        // Auth
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}