using CarStoreManager.Application.Common;
using CarStoreManager.Application.Interfaces;
using CarStoreManager.Application.Services;
using CarStoreManager.Domain.Repositories;
using CarStoreManager.Infrastructure.DependencyInjection;
using CarStoreManager.Infrastructure.Repositories;
using CarStoreManager.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarStoreManager.Geradores.Nucleo;

/// <summary>
/// Monta o mesmo grafo de DI que o Web usa (Infrastructure + os serviços de
/// Application que o Web registra em cima) apontando para o mesmo
/// carstore.db local, para que os Geradores criem dados passando pelas
/// MESMAS regras de negócio e validações que a aplicação usa — nunca
/// inserindo direto via SQL/EF.
/// </summary>
public static class ComposicaoServicos
{
    public static IServiceProvider Construir()
    {
        // Carrega appsettings.json + appsettings.Development.json do Web na
        // mesma ordem de precedência que o ASP.NET Core usaria — é de lá que
        // vem a connection string do SQLite (ver
        // InfrastructureServiceCollection.AddInfrastructure).
        var configuracaoFinal = new ConfigurationBuilder()
            .AddJsonFile(CaminhosProjeto.AppSettingsWeb(), optional: false)
            .AddJsonFile(CaminhosProjeto.AppSettingsDevelopment(), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuracaoFinal);

        // Mesmos registros extras que Web/Extensions/ApplicationServiceCollection
        // adiciona por cima do Infrastructure (ver Web/Extensions/ApplicationServiceCollection.cs).
        services.AddScoped<IVeiculoVendaService, VeiculoVendaService>();
        services.AddScoped<IVeiculoConsignacaoService, VeiculoConsignacaoService>();
        services.AddScoped<IVendedorService, VendedorService>();
        services.AddScoped<IPropostaVendaService, PropostaVendaService>();
        services.AddScoped<IVeiculoClienteService, VeiculoClienteService>();
        services.AddScoped<IMecanicoService, MecanicoService>();
        services.AddScoped<IComponenteService, ComponenteService>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        // Fora de um host ASP.NET Core real — algumas dependências transitivas
        // (ArquivoStorageService, via IFotoService) esperam IWebHostEnvironment.
        services.AddSingleton<IWebHostEnvironment>(new AmbienteWebFake());

        services.Configure<JwtSettings>(configuracaoFinal.GetSection("Jwt"));

        return services.BuildServiceProvider();
    }
}
