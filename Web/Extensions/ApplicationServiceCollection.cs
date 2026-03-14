using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;

namespace CarStoreManager.Web.Extensions;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IVeiculoService, VeiculoService>();
        services.AddScoped<IPecaService, PecaService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IPropostaVendaService, PropostaVendaService>();

        return services;
    }
}