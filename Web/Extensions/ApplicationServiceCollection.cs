using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;
using CarStoreManager.Application.Services.Integracoes;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Infrastructure.Services;
using CarStoreManager.Web.Tours;

namespace CarStoreManager.Web.Extensions;

public static class ApplicationServiceCollection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Shared
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IClienteConsultaService, CarStoreManager.Application.Services.ClienteConsultaService>();

        // Concessionaria
        services.AddScoped<IVeiculoVendaService, VeiculoVendaService>();
        services.AddScoped<IVeiculoConsignacaoService, VeiculoConsignacaoService>();
        services.AddScoped<IVendedorService, VendedorService>();
        services.AddScoped<IPropostaVendaService, PropostaVendaService>();
        services.AddScoped<ITestDriveService, CarStoreManager.Application.Services.TestDriveService>();

        // Oficina
        services.AddScoped<IVeiculoClienteService, VeiculoClienteService>();
        services.AddScoped<IMecanicoService, MecanicoService>();
        services.AddScoped<IComponenteService, ComponenteService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IConsultaPublicaService, CarStoreManager.Application.Services.ConsultaPublicaService>();

        // Auth
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Integrações — Mercado Livre
        services.AddScoped<MercadoLivreTokenHelper>();
        services.AddScoped<MercadoLivreCatalogoService>();
        services.AddScoped<IConstrutorPayloadAnuncio, ComponentePayloadBuilder>();
        services.AddScoped<IConstrutorPayloadAnuncio, VeiculoPayloadBuilder>();
        services.AddScoped<IMercadoLivrePublicacaoService, MercadoLivrePublicacaoService>();
        services.AddScoped<IMercadoLivreSincronizacaoService, MercadoLivreSincronizacaoService>();

        // Tour guiado (manual do usuário embutido)
        services.AddScoped<TourEstadoService>();

        return services;
    }
}